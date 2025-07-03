namespace VoicePOC.Services;

public interface IOpenAIService
{
    Task<string> GetCompletionAsync(string userMessage, List<string> conversationHistory);
}