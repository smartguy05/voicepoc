using System.Collections.Concurrent;
using VoicePOC.Models;

namespace VoicePOC.Services;

public class CallSessionService : ICallSessionService
{
    private readonly ConcurrentDictionary<string, CallSession> _sessions = new();
    private readonly ILogger<CallSessionService> _logger;

    public CallSessionService(ILogger<CallSessionService> logger)
    {
        _logger = logger;
    }

    public CallSession CreateSession(string callSid, string from, string to)
    {
        var session = new CallSession
        {
            CallSid = callSid,
            From = from,
            To = to,
            StartTime = DateTime.UtcNow
        };

        _sessions.TryAdd(callSid, session);
        _logger.LogInformation("Created new call session: {CallSid} from {From} to {To}", callSid, from, to);
        
        return session;
    }

    public CallSession? GetSession(string callSid)
    {
        _sessions.TryGetValue(callSid, out var session);
        return session;
    }

    public void UpdateSession(string callSid, string userInput, string assistantResponse)
    {
        if (_sessions.TryGetValue(callSid, out var session))
        {
            session.ConversationHistory.Add($"User: {userInput}");
            session.ConversationHistory.Add($"Assistant: {assistantResponse}");
            _logger.LogInformation("Updated conversation history for call: {CallSid}", callSid);
        }
    }

    public void EndSession(string callSid)
    {
        if (_sessions.TryRemove(callSid, out var session))
        {
            session.Status = "ended";
            _logger.LogInformation("Ended call session: {CallSid}, Duration: {Duration}", 
                callSid, DateTime.UtcNow - session.StartTime);
        }
    }

    public IEnumerable<CallSession> GetAllActiveSessions()
    {
        return _sessions.Values.Where(s => s.Status == "active");
    }
}