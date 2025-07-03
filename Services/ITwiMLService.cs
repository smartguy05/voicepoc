using Twilio.TwiML;

namespace VoicePOC.Services;

public interface ITwiMLService
{
    VoiceResponse CreateWelcomeResponse();
    VoiceResponse CreateContinueConversationResponse(string assistantMessage);
    VoiceResponse CreateErrorResponse(string errorMessage);
    VoiceResponse CreateGoodbyeResponse();
}