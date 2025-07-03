using VoicePOC.Models;

namespace VoicePOC.Services;

public interface IStreamingSessionService
{
    Task StartStreamingAsync(string callSid, string userInput, List<string> conversationHistory);
    StreamingSession? GetStreamingSession(string callSid);
    string? GetNextChunk(string callSid);
    bool IsStreamingComplete(string callSid);
    void CompleteStreaming(string callSid);
    void CleanupSession(string callSid);
}