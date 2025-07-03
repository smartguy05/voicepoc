using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using VoicePOC.Models;
using System.Runtime.CompilerServices;

namespace VoicePOC.Services;

public class OpenAIStreamingService : IOpenAIStreamingService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIConfig _config;
    private readonly ILogger<OpenAIStreamingService> _logger;

    public OpenAIStreamingService(HttpClient httpClient, IOptions<OpenAIConfig> config, ILogger<OpenAIStreamingService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> GetStreamingCompletionAsync(string userMessage, List<string> conversationHistory, CancellationToken cancellationToken = default)
    {
        var response = new StringBuilder();
        
        await foreach (var token in GetStreamingTokensAsync(userMessage, conversationHistory, cancellationToken))
        {
            response.Append(token);
        }
        
        return response.ToString().Trim();
    }

    public async IAsyncEnumerable<string> GetStreamingTokensAsync(string userMessage, List<string> conversationHistory, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var hasYieldedChunks = false;
        
        await foreach (var chunk in GetStreamingTokensInternalAsync(userMessage, conversationHistory, cancellationToken))
        {
            hasYieldedChunks = true;
            yield return chunk;
        }
        
        // If no chunks were yielded due to error, provide fallback
        if (!hasYieldedChunks)
        {
            yield return "I'm sorry, I'm having trouble processing your request right now. Please try again.";
        }
    }

    private async IAsyncEnumerable<string> GetStreamingTokensInternalAsync(string userMessage, List<string> conversationHistory, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = new List<object>
        {
            new { role = "system", content = "You are a helpful voice assistant. Keep your responses concise and conversational, suitable for voice interaction. Respond in a friendly and engaging manner. Aim for responses that are 1-2 sentences for better voice flow." }
        };

        // Add conversation history
        foreach (var historyItem in conversationHistory)
        {
            if (historyItem.StartsWith("User: "))
            {
                messages.Add(new { role = "user", content = historyItem.Substring(6) });
            }
            else if (historyItem.StartsWith("Assistant: "))
            {
                messages.Add(new { role = "assistant", content = historyItem.Substring(11) });
            }
        }

        // Add current user message
        messages.Add(new { role = "user", content = userMessage });

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = messages,
            max_tokens = 100,
            temperature = 0.7,
            stream = true // Enable streaming
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_config.BaseUrl}/chat/completions")
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI streaming API request failed with status: {StatusCode}", response.StatusCode);
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var buffer = new StringBuilder();
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6);
                
                if (data == "[DONE]")
                    break;

                if (TryParseJson(data, out var jsonData))
                {
                    if (jsonData.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var choice = choices[0];
                        if (choice.TryGetProperty("delta", out var delta) && 
                            delta.TryGetProperty("content", out var contentProperty))
                        {
                            var token = contentProperty.GetString();
                            if (!string.IsNullOrEmpty(token))
                            {
                                buffer.Append(token);
                                
                                // Yield complete words or sentences for better voice synthesis
                                if (token.Contains(" ") || token.Contains(".") || token.Contains("!") || token.Contains("?"))
                                {
                                    var completeText = buffer.ToString();
                                    if (!string.IsNullOrWhiteSpace(completeText))
                                    {
                                        yield return completeText;
                                        buffer.Clear();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to parse streaming response data: {Data}", data);
                }
            }
        }

        // Yield any remaining content
        if (buffer.Length > 0)
        {
            var remainingText = buffer.ToString();
            if (!string.IsNullOrWhiteSpace(remainingText))
            {
                yield return remainingText;
            }
        }
    }

    private bool TryParseJson(string data, out JsonElement jsonData)
    {
        try
        {
            jsonData = JsonSerializer.Deserialize<JsonElement>(data);
            return true;
        }
        catch (JsonException)
        {
            jsonData = default;
            return false;
        }
    }
}