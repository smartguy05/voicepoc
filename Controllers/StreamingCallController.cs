using Microsoft.AspNetCore.Mvc;
using VoicePOC.Services;

namespace VoicePOC.Controllers;

[ApiController]
[Route("api/streaming")]
public class StreamingCallController : ControllerBase
{
    private readonly ILogger<StreamingCallController> _logger;
    private readonly ICallSessionService _callSessionService;
    private readonly IStreamingSessionService _streamingSessionService;
    private readonly IStreamingTwiMLService _streamingTwiMLService;
    private readonly ITwiMLService _twiMLService;

    public StreamingCallController(
        ILogger<StreamingCallController> logger,
        ICallSessionService callSessionService,
        IStreamingSessionService streamingSessionService,
        IStreamingTwiMLService streamingTwiMLService,
        ITwiMLService twiMLService)
    {
        _logger = logger;
        _callSessionService = callSessionService;
        _streamingSessionService = streamingSessionService;
        _streamingTwiMLService = streamingTwiMLService;
        _twiMLService = twiMLService;
    }

    [HttpPost("answer")]
    public IActionResult Answer()
    {
        var callSid = Request.Form["CallSid"];
        var from = Request.Form["From"];
        var to = Request.Form["To"];
        
        _logger.LogInformation("Streaming: Incoming call answered: {CallSid} from {From} to {To}", callSid, from, to);
        
        // Create a new call session (reusing existing service)
        _callSessionService.CreateSession(callSid, from, to);
        
        // Generate welcome TwiML response (reusing existing service)
        var response = _twiMLService.CreateWelcomeResponse();
        
        return Content(response.ToString(), "text/xml");
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessStreaming()
    {
        var startTime = DateTime.UtcNow;
        var speechResult = Request.Form["SpeechResult"];
        var callSid = Request.Form["CallSid"];
        
        _logger.LogInformation("Streaming: Processing speech: {SpeechResult} for call: {CallSid}", speechResult, callSid);
        
        if (string.IsNullOrEmpty(speechResult))
        {
            var errorResponse = _twiMLService.CreateErrorResponse("I didn't understand what you said. Please try again.");
            return Content(errorResponse.ToString(), "text/xml");
        }

        // Get conversation history
        var session = _callSessionService.GetSession(callSid);
        var conversationHistory = session?.ConversationHistory ?? new List<string>();
        
        // Start streaming process in background
        await _streamingSessionService.StartStreamingAsync(callSid, speechResult, conversationHistory);
        
        // Return immediate processing response
        var response = _streamingTwiMLService.CreateStreamingProcessingResponse();
        
        var processingTime = DateTime.UtcNow - startTime;
        _logger.LogInformation("Streaming: Processing initiated for call {CallSid} in {ProcessingTime}ms", callSid, processingTime.TotalMilliseconds);
        
        return Content(response.ToString(), "text/xml");
    }

    [HttpPost("process-stream")]
    public IActionResult ProcessStream()
    {
        var callSid = Request.Form["CallSid"];
        
        _logger.LogInformation("Streaming: Process stream called for call: {CallSid}", callSid);
        
        // Short delay to ensure first chunk is ready
        Thread.Sleep(200);
        
        // Get next chunk
        var chunk = _streamingSessionService.GetNextChunk(callSid);
        var isComplete = _streamingSessionService.IsStreamingComplete(callSid);
        
        if (chunk != null || isComplete)
        {
            var response = _streamingTwiMLService.CreateStreamingChunkResponse(chunk ?? "", isComplete, callSid);
            return Content(response.ToString(), "text/xml");
        }
        else
        {
            // No chunk ready yet, wait a moment and try again
            var response = _streamingTwiMLService.CreateStreamingProcessingResponse();
            return Content(response.ToString(), "text/xml");
        }
    }

    [HttpGet("next-chunk")]
    public IActionResult NextChunk(string callSid)
    {
        _logger.LogInformation("Streaming: Next chunk requested for call: {CallSid}", callSid);
        
        // Small delay to allow for chunk processing
        Thread.Sleep(100);
        
        var chunk = _streamingSessionService.GetNextChunk(callSid);
        var isComplete = _streamingSessionService.IsStreamingComplete(callSid);
        
        if (isComplete && chunk == null)
        {
            // Streaming is complete, clean up and continue conversation
            _streamingSessionService.CleanupSession(callSid);
            var response = _streamingTwiMLService.CreateStreamingCompleteResponse();
            return Content(response.ToString(), "text/xml");
        }
        
        var chunkResponse = _streamingTwiMLService.CreateStreamingChunkResponse(chunk ?? "", isComplete, callSid);
        return Content(chunkResponse.ToString(), "text/xml");
    }

    [HttpPost("hangup")]
    public IActionResult Hangup()
    {
        var callSid = Request.Form["CallSid"];
        
        _logger.LogInformation("Streaming: Call ended: {CallSid}", callSid);
        
        // Clean up both session types
        _callSessionService.EndSession(callSid);
        _streamingSessionService.CleanupSession(callSid);
        
        return Ok();
    }
}