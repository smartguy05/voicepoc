using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using VoicePOC.Models;

namespace VoicePOC.Services;

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIConfig _config;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(HttpClient httpClient, IOptions<OpenAIConfig> config, ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> GetCompletionAsync(string userMessage, List<string> conversationHistory)
    {
        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = "You are a helpful voice assistant. Keep your responses concise and conversational, suitable for voice interaction. Respond in a friendly and engaging manner." }
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
                max_tokens = 100, // Reduced for faster responses
                temperature = 0.7,
                stream = false // Could enable streaming later
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

            var response = await _httpClient.PostAsync($"{_config.BaseUrl}/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API request failed with status: {StatusCode}", response.StatusCode);
                return "I'm sorry, I'm having trouble processing your request right now. Please try again.";
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var openAIResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var assistantMessage = openAIResponse
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return assistantMessage ?? "I'm sorry, I didn't understand that. Could you please try again?";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            return "I'm sorry, I'm experiencing technical difficulties. Please try again later.";
        }
    }
}