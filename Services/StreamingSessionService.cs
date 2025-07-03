using System.Collections.Concurrent;
using VoicePOC.Models;

namespace VoicePOC.Services;

public class StreamingSessionService : IStreamingSessionService
{
    private readonly ConcurrentDictionary<string, StreamingSession> _streamingSessions = new();
    private readonly IOpenAIStreamingService _openAIStreamingService;
    private readonly ICallSessionService _callSessionService;
    private readonly ILogger<StreamingSessionService> _logger;

    public StreamingSessionService(
        IOpenAIStreamingService openAIStreamingService,
        ICallSessionService callSessionService,
        ILogger<StreamingSessionService> logger)
    {
        _openAIStreamingService = openAIStreamingService;
        _callSessionService = callSessionService;
        _logger = logger;
    }

    public async Task StartStreamingAsync(string callSid, string userInput, List<string> conversationHistory)
    {
        var session = new StreamingSession
        {
            CallSid = callSid,
            CurrentUserInput = userInput,
            StartTime = DateTime.UtcNow,
            IsProcessing = true,
            IsComplete = false
        };

        _streamingSessions.TryAdd(callSid, session);
        _logger.LogInformation("Started streaming session for call: {CallSid}", callSid);

        // Start background streaming task
        _ = Task.Run(async () =>
        {
            try
            {
                await foreach (var chunk in _openAIStreamingService.GetStreamingTokensAsync(userInput, conversationHistory))
                {
                    if (_streamingSessions.TryGetValue(callSid, out var currentSession))
                    {
                        currentSession.ResponseChunks.Enqueue(chunk);
                        currentSession.FullResponse += chunk;
                        _logger.LogDebug("Added chunk to session {CallSid}: {Chunk}", callSid, chunk);
                    }
                    else
                    {
                        // Session was cleaned up, stop processing
                        break;
                    }
                }

                // Mark as complete
                if (_streamingSessions.TryGetValue(callSid, out var finalSession))
                {
                    finalSession.IsProcessing = false;
                    finalSession.IsComplete = true;
                    
                    // Update main call session with complete response
                    _callSessionService.UpdateSession(callSid, userInput, finalSession.FullResponse);
                    
                    _logger.LogInformation("Completed streaming for call: {CallSid}, total response length: {Length}", 
                        callSid, finalSession.FullResponse.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during streaming for call: {CallSid}", callSid);
                
                if (_streamingSessions.TryGetValue(callSid, out var errorSession))
                {
                    errorSession.ResponseChunks.Enqueue("I'm sorry, there was an error processing your request.");
                    errorSession.IsProcessing = false;
                    errorSession.IsComplete = true;
                }
            }
        });

        // Wait a moment for first chunk to be available
        await Task.Delay(100);
    }

    public StreamingSession? GetStreamingSession(string callSid)
    {
        _streamingSessions.TryGetValue(callSid, out var session);
        return session;
    }

    public string? GetNextChunk(string callSid)
    {
        if (_streamingSessions.TryGetValue(callSid, out var session))
        {
            if (session.ResponseChunks.TryDequeue(out var chunk))
            {
                _logger.LogDebug("Retrieved chunk for call {CallSid}: {Chunk}", callSid, chunk);
                return chunk;
            }
        }
        return null;
    }

    public bool IsStreamingComplete(string callSid)
    {
        if (_streamingSessions.TryGetValue(callSid, out var session))
        {
            return session.IsComplete && session.ResponseChunks.Count == 0;
        }
        return true; // If no session, consider it complete
    }

    public void CompleteStreaming(string callSid)
    {
        if (_streamingSessions.TryGetValue(callSid, out var session))
        {
            session.IsComplete = true;
            _logger.LogInformation("Marked streaming complete for call: {CallSid}", callSid);
        }
    }

    public void CleanupSession(string callSid)
    {
        if (_streamingSessions.TryRemove(callSid, out var session))
        {
            _logger.LogInformation("Cleaned up streaming session for call: {CallSid}", callSid);
        }
    }
}