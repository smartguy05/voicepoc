using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace VoicePOC.Services;

public class StreamingTwiMLService : IStreamingTwiMLService
{
    private readonly ILogger<StreamingTwiMLService> _logger;

    public StreamingTwiMLService(ILogger<StreamingTwiMLService> logger)
    {
        _logger = logger;
    }

    public VoiceResponse CreateStreamingProcessingResponse()
    {
        var response = new VoiceResponse();
        
        // Immediate acknowledgment to reduce perceived latency
        response.Say("Let me process that for you.", voice: Say.VoiceEnum.Alice);
        
        // Redirect to streaming endpoint for actual processing
        response.Redirect(new Uri("/api/streaming/process-stream", UriKind.Relative));
        
        _logger.LogInformation("Generated streaming processing TwiML response");
        return response;
    }

    public VoiceResponse CreateStreamingChunkResponse(string textChunk, bool isComplete, string callSid)
    {
        var response = new VoiceResponse();
        
        // Speak the text chunk
        if (!string.IsNullOrWhiteSpace(textChunk))
        {
            response.Say(textChunk, voice: Say.VoiceEnum.Alice);
        }
        
        if (isComplete)
        {
            // Continue conversation after complete response
            var gather = new Gather(
                input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
                speechTimeout: "2",
                timeout: 8,
                action: new Uri("/api/streaming/answer", UriKind.Relative),
                method: "POST"
            );
            
            gather.Say("What else would you like to know?", voice: Say.VoiceEnum.Alice);
            response.Append(gather);
            
            // Fallback
            response.Say("Thank you for using the voice assistant. Goodbye!");
            response.Hangup();
        }
        else
        {
            // Continue to next chunk
            response.Redirect(new Uri($"/api/streaming/next-chunk?callSid={callSid}", UriKind.Relative));
        }
        
        _logger.LogInformation("Generated streaming chunk TwiML response: complete={IsComplete}", isComplete);
        return response;
    }

    public VoiceResponse CreateStreamingCompleteResponse()
    {
        var response = new VoiceResponse();
        
        // Continue the conversation
        var gather = new Gather(
            input: new List<Gather.InputEnum> { Gather.InputEnum.Speech },
            speechTimeout: "2",
            timeout: 8,
            action: new Uri("/api/streaming/answer", UriKind.Relative),
            method: "POST"
        );
        
        gather.Say("What else can I help you with?", voice: Say.VoiceEnum.Alice);
        response.Append(gather);
        
        // Fallback
        response.Say("Thank you for calling. Have a great day!");
        response.Hangup();
        
        _logger.LogInformation("Generated streaming complete TwiML response");
        return response;
    }
}