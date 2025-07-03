using VoicePOC.Models;

namespace VoicePOC.Services;

public interface ICallSessionService
{
    CallSession CreateSession(string callSid, string from, string to);
    CallSession? GetSession(string callSid);
    void UpdateSession(string callSid, string userInput, string assistantResponse);
    void EndSession(string callSid);
    IEnumerable<CallSession> GetAllActiveSessions();
}