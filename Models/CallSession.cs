namespace VoicePOC.Models;

public class CallSession
{
    public string CallSid { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public List<string> ConversationHistory { get; set; } = new();
    public string Status { get; set; } = "active";
}