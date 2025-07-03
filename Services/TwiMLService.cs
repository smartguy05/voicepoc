using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace VoicePOC.Services;

public class TwiMLService : ITwiMLService
{
    private readonly ILogger<TwiMLService> _logger;

    public TwiMLService(ILogger<TwiMLService> logger)
    {
        _logger = logger;
    }

    public VoiceResponse CreateWelcomeResponse()
    {
        var response = new VoiceResponse();
        
        var gather = new Gather(
            input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
            speechTimeout: "3",
            timeout: 10,
            action: new Uri("/api/call/process", UriKind.Relative),
            method: "POST"
        );
        
        gather.Say("Hello! I'm your AI voice assistant. Please say something to get started.");
        response.Append(gather);
        
        // Fallback if no input is received
        response.Say("I didn't hear anything. Please try calling back.");
        response.Hangup();
        
        _logger.LogInformation("Generated welcome TwiML response");
        return response;
    }

    public VoiceResponse CreateContinueConversationResponse(string assistantMessage)
    {
        var response = new VoiceResponse();
        
        // Speak the assistant's response
        response.Say(assistantMessage);
        
        // Continue the conversation with shorter timeout for better UX
        var gather = new Gather(
            input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
            speechTimeout: "2", // Reduced from 3 to 2 seconds
            timeout: 8, // Reduced from 10 to 8 seconds
            action: new Uri("/api/call/process", UriKind.Relative),
            method: "POST"
        );
        
        gather.Say("What else would you like to know?");
        response.Append(gather);
        
        // Fallback if no more input
        response.Say("Thank you for using the voice assistant. Goodbye!");
        response.Hangup();
        
        _logger.LogInformation("Generated continue conversation TwiML response");
        return response;
    }

    public VoiceResponse CreateThinkingResponse()
    {
        var response = new VoiceResponse();
        
        response.Say("Let me think about that for a moment.");
        response.Redirect(new Uri("/api/call/process-delayed", UriKind.Relative));
        
        _logger.LogInformation("Generated thinking TwiML response");
        return response;
    }

    public VoiceResponse CreateErrorResponse(string errorMessage)
    {
        var response = new VoiceResponse();
        
        response.Say(errorMessage);
        
        // Redirect back to gather more input
        response.Redirect(new Uri("/api/call/answer", UriKind.Relative));
        
        _logger.LogInformation("Generated error TwiML response: {ErrorMessage}", errorMessage);
        return response;
    }

    public VoiceResponse CreateGoodbyeResponse()
    {
        var response = new VoiceResponse();
        
        response.Say("Thank you for using the AI voice assistant. Have a great day!");
        response.Hangup();
        
        _logger.LogInformation("Generated goodbye TwiML response");
        return response;
    }
}