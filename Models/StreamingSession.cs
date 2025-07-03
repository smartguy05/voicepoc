namespace VoicePOC.Models;

public class StreamingSession
{
    public string CallSid { get; set; } = string.Empty;
    public Queue<string> ResponseChunks { get; set; } = new();
    public string CurrentUserInput { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public bool IsProcessing { get; set; }
    public bool IsComplete { get; set; }
    public string FullResponse { get; set; } = string.Empty;
}