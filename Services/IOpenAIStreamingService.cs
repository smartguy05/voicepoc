namespace VoicePOC.Services;

public interface IOpenAIStreamingService
{
    Task<string> GetStreamingCompletionAsync(string userMessage, List<string> conversationHistory, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> GetStreamingTokensAsync(string userMessage, List<string> conversationHistory, CancellationToken cancellationToken = default);
}