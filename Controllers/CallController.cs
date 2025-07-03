using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using VoicePOC.Services;

namespace VoicePOC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CallController : ControllerBase
{
    private readonly ILogger<CallController> _logger;
    private readonly ICallSessionService _callSessionService;
    private readonly IOpenAIService _openAIService;
    private readonly ITwiMLService _twiMLService;

    public CallController(ILogger<CallController> logger, ICallSessionService callSessionService, IOpenAIService openAIService, ITwiMLService twiMLService)
    {
        _logger = logger;
        _callSessionService = callSessionService;
        _openAIService = openAIService;
        _twiMLService = twiMLService;
    }

    [HttpPost("answer")]
    public IActionResult Answer()
    {
        var callSid = Request.Form["CallSid"];
        var from = Request.Form["From"];
        var to = Request.Form["To"];
        
        _logger.LogInformation("Incoming call answered: {CallSid} from {From} to {To}", callSid, from, to);
        
        // Create a new call session
        _callSessionService.CreateSession(callSid, from, to);
        
        // Generate welcome TwiML response
        var response = _twiMLService.CreateWelcomeResponse();
        
        return Content(response.ToString(), "text/xml");
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process()
    {
        var startTime = DateTime.UtcNow;
        var speechResult = Request.Form["SpeechResult"];
        var callSid = Request.Form["CallSid"];
        
        _logger.LogInformation("Processing speech: {SpeechResult} for call: {CallSid}", speechResult, callSid);
        
        VoiceResponse response;
        
        if (string.IsNullOrEmpty(speechResult))
        {
            response = _twiMLService.CreateErrorResponse("I didn't understand what you said. Please try again.");
        }
        else
        {
            // Get conversation history
            var session = _callSessionService.GetSession(callSid);
            var conversationHistory = session?.ConversationHistory ?? new List<string>();
            
            // Get AI response
            var assistantResponse = await _openAIService.GetCompletionAsync(speechResult, conversationHistory);
            
            // Update session with conversation history
            _callSessionService.UpdateSession(callSid, speechResult, assistantResponse);
            
            // Generate TwiML response to continue conversation
            response = _twiMLService.CreateContinueConversationResponse(assistantResponse);
        }
        
        var totalTime = DateTime.UtcNow - startTime;
        _logger.LogInformation("Total processing time for call {CallSid}: {TotalTime}ms", callSid, totalTime.TotalMilliseconds);
        
        return Content(response.ToString(), "text/xml");
    }

    [HttpPost("hangup")]
    public IActionResult Hangup()
    {
        var callSid = Request.Form["CallSid"];
        
        _logger.LogInformation("Call ended: {CallSid}", callSid);
        
        // End the call session
        _callSessionService.EndSession(callSid);
        
        return Ok();
    }
}