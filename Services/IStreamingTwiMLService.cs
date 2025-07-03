using Twilio.TwiML;

namespace VoicePOC.Services;

public interface IStreamingTwiMLService
{
    VoiceResponse CreateStreamingProcessingResponse();
    VoiceResponse CreateStreamingChunkResponse(string textChunk, bool isComplete, string callSid);
    VoiceResponse CreateStreamingCompleteResponse();
}