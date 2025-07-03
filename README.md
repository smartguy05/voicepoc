# Twilio + OpenAI Voice Assistant POC

A proof-of-concept application that enables voice-to-voice conversations with an AI assistant via Twilio phone calls. Users can call a phone number, speak naturally, and receive intelligent responses from OpenAI's GPT model through voice synthesis.

## 🏗️ Architecture Overview

```
Phone Call → Twilio → Your App → OpenAI → Your App → Twilio → Voice Response
           (STT)    (Process)   (LLM)    (TwiML)  (TTS)
```

### Core Components

- **ASP.NET Core 9 Web API**: Handles HTTP webhooks from Twilio
- **Twilio Programmable Voice**: Speech-to-text, text-to-speech, call management
- **OpenAI GPT API**: Natural language processing and response generation
- **In-Memory Session Store**: Tracks conversation history during calls

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Twilio Account](https://www.twilio.com/console) with a phone number
- [OpenAI API Account](https://platform.openai.com/account/api-keys)
- [ngrok](https://ngrok.com/) for local development tunneling

## 🚀 Quick Start

### 1. Clone and Setup

```bash
cd VoicePOC
dotnet restore
```

### 2. Configure API Keys

The application is already configured with your credentials in `appsettings.Development.json`:
- ✅ Twilio Account SID and Auth Token
- ✅ OpenAI API Key

### 3. Run the Application

```bash
dotnet run --urls=http://localhost:5000
```

The application will start and listen on `http://localhost:5000`.

### 4. Setup ngrok Tunnel

In a separate terminal:

```bash
# First time setup - authenticate with ngrok
./ngrok authtoken YOUR_NGROK_AUTH_TOKEN

# Start tunnel
./ngrok http 5000
```

Copy the public HTTPS URL (e.g., `https://abc123.ngrok.io`).

### 5. Configure Twilio Webhook

1. Go to [Twilio Console > Phone Numbers](https://console.twilio.com/us1/develop/phone-numbers/manage/incoming)
2. Click on your phone number
3. Set the Webhook URL to: `https://abc123.ngrok.io/api/call/answer`
4. Set HTTP method to `POST`
5. Save configuration

### 6. Test the System

Call your Twilio phone number and start speaking! 🎉

## 📡 API Endpoints

### Standard (Non-Streaming) Endpoints

### `POST /api/call/answer`
- **Purpose**: Initial webhook when call is answered
- **Input**: Twilio call parameters (CallSid, From, To)
- **Output**: TwiML with welcome message and speech gathering

### `POST /api/call/process` 
- **Purpose**: Process speech input and generate AI responses
- **Input**: Speech transcription from Twilio
- **Output**: TwiML with AI response and continuation prompt

### `POST /api/call/hangup`
- **Purpose**: Clean up call session when call ends
- **Input**: CallSid
- **Output**: HTTP 200 OK

### Streaming Endpoints (Lower Latency)

### `POST /api/streaming/answer`
- **Purpose**: Initial webhook for streaming implementation
- **Input**: Twilio call parameters (CallSid, From, To)
- **Output**: TwiML with welcome message and speech gathering

### `POST /api/streaming/process`
- **Purpose**: Initiate streaming AI response processing
- **Input**: Speech transcription from Twilio
- **Output**: TwiML with immediate processing acknowledgment

### `POST /api/streaming/process-stream`
- **Purpose**: Handle streaming chunk delivery
- **Input**: CallSid
- **Output**: TwiML with first response chunk or continuation

### `GET /api/streaming/next-chunk?callSid={callSid}`
- **Purpose**: Retrieve next streaming response chunk
- **Input**: CallSid as query parameter
- **Output**: TwiML with next chunk or completion signal

### `POST /api/streaming/hangup`
- **Purpose**: Clean up streaming and regular call sessions
- **Input**: CallSid
- **Output**: HTTP 200 OK

## 🏗️ Project Structure

```
VoicePOC/
├── Controllers/
│   └── CallController.cs           # Main API endpoints
├── Models/
│   ├── CallSession.cs             # Call session data model
│   ├── TwilioConfig.cs            # Twilio configuration
│   └── OpenAIConfig.cs            # OpenAI configuration
├── Services/
│   ├── ICallSessionService.cs      # Session management interface
│   ├── CallSessionService.cs      # In-memory session store
│   ├── IOpenAIService.cs          # OpenAI integration interface
│   ├── OpenAIService.cs           # OpenAI API client
│   ├── ITwiMLService.cs           # TwiML generation interface
│   └── TwiMLService.cs            # Dynamic TwiML responses
├── appsettings.Development.json    # Configuration file
├── Program.cs                     # Application startup
└── README.md                      # This file
```

## 🔧 Configuration

### Twilio Settings
```json
{
  "Twilio": {
    "AccountSid": "YOUR_ACCOUNT_SID",
    "AuthToken": "YOUR_AUTH_TOKEN"
  }
}
```

### OpenAI Settings
```json
{
  "OpenAI": {
    "ApiKey": "YOUR_API_KEY",
    "BaseUrl": "https://api.openai.com/v1"
  }
}
```

## 🎯 How It Works

### Standard Call Flow
1. **Incoming Call**: User dials Twilio number
2. **Webhook Trigger**: Twilio sends POST to `/api/call/answer`
3. **Session Creation**: App creates new call session with unique ID
4. **Welcome Message**: TwiML responds with greeting and speech gathering
5. **Speech Recognition**: Twilio converts speech to text
6. **AI Processing**: Text sent to OpenAI for intelligent response (1-4 seconds)
7. **Response Generation**: AI response converted to TwiML `<Say>` command
8. **Conversation Loop**: Process repeats until call ends

### Streaming Call Flow (Lower Latency)
1. **Incoming Call**: User dials Twilio number  
2. **Webhook Trigger**: Twilio sends POST to `/api/streaming/answer`
3. **Session Creation**: App creates new call session with unique ID
4. **Welcome Message**: TwiML responds with greeting and speech gathering
5. **Speech Recognition**: Twilio converts speech to text
6. **Immediate Acknowledgment**: "Let me process that for you" (reduces perceived latency)
7. **Background AI Processing**: OpenAI streaming starts in background task
8. **Chunked Response Delivery**: AI response delivered in real-time chunks via multiple TwiML calls
9. **Conversation Loop**: Process repeats with improved responsiveness

### Latency Comparison
- **Standard Mode**: 3-9 seconds per interaction
- **Streaming Mode**: 1-3 seconds for first response chunk, then continuous delivery

### Session Management
- Each call gets a unique session tracked by Twilio's CallSid
- Conversation history maintained in memory during call
- Sessions automatically cleaned up on hangup

### Error Handling
- Failed speech recognition redirects to retry
- OpenAI API errors return fallback messages
- Automatic fallback responses if no input detected

## 🛠️ Development

### Running in Development
```bash
# Start the application
dotnet run --urls=http://localhost:5000

# In another terminal, start ngrok
./ngrok http 5000

# Update Twilio webhook URL when ngrok URL changes
```

### Debugging
- Application logs appear in console and `app.log`
- Call session data logged with each interaction
- TwiML responses logged for debugging voice flow

### Common Issues

**"Authentication failed" with ngrok**
- Sign up at https://dashboard.ngrok.com/signup
- Get auth token from https://dashboard.ngrok.com/get-started/your-authtoken
- Run: `./ngrok authtoken YOUR_TOKEN`

**"No response from webhook"**
- Ensure ngrok is running and URL is correct
- Check that Twilio webhook URL includes full path: `/api/call/answer`
- Verify application is running on localhost:5000

**"OpenAI API errors"**
- Verify API key is valid and has credits
- Check OpenAI usage limits
- Monitor rate limiting

## 🚀 Deployment

### Azure App Service
1. Create App Service with .NET 9 runtime
2. Set application settings for Twilio and OpenAI configs
3. Deploy code via Visual Studio or GitHub Actions
4. Update Twilio webhook to production URL

### AWS Lambda
1. Use AWS Toolkit for .NET
2. Package as Lambda function
3. Set environment variables for configuration
4. Configure API Gateway for webhook endpoints

## 📊 Monitoring

### Application Logs
```bash
# View real-time logs
tail -f app.log

# Search for specific call
grep "CallSid" app.log
```

### Twilio Console
- Monitor call volumes and success rates
- View webhook success/failure rates
- Check call recordings and transcriptions

### OpenAI Usage
- Monitor API usage in OpenAI dashboard
- Track token consumption
- Set up usage alerts

## 🔐 Security Considerations

### Production Deployment
- Use environment variables instead of appsettings.json
- Implement Twilio request validation
- Add rate limiting for webhook endpoints
- Use HTTPS only
- Implement proper error handling without exposing internals

### Webhook Validation (Recommended)
```csharp
// Add to CallController for production
[HttpPost("answer")]
public IActionResult Answer()
{
    // Validate request is from Twilio
    var requestValidator = new RequestValidator(authToken);
    var isValid = requestValidator.Validate(Request);
    
    if (!isValid)
        return Unauthorized();
    
    // ... rest of method
}
```

## 📈 Performance Optimization

### Caching
- Consider Redis for session storage in production
- Cache OpenAI responses for common queries
- Implement conversation context trimming

### Latency
- Use async/await throughout
- Consider OpenAI streaming responses
- Optimize TwiML generation

### Scaling
- Use dependency injection for stateless services
- Consider message queues for high-volume processing
- Implement connection pooling for external APIs

## 🧪 Testing

### Manual Testing
1. Call the phone number
2. Say "Hello, how are you?"
3. Verify AI responds appropriately
4. Test conversation continuity
5. Test error scenarios (silence, background noise)

### Automated Testing
```bash
# Run unit tests (when implemented)
dotnet test

# Test webhook endpoints directly
curl -X POST http://localhost:5000/api/call/answer \
  -d "CallSid=test&From=+1234567890&To=+0987654321"
```

## 🔄 Future Enhancements

### Immediate Improvements
- [ ] Add webhook request validation
- [ ] Implement proper error responses
- [ ] Add conversation context limits
- [ ] Include call duration tracking

### Advanced Features
- [ ] Multi-language support
- [ ] Custom voice personalities
- [ ] Integration with CRM systems
- [ ] Call recording and analysis
- [ ] Real-time conversation monitoring

## 📞 Support

### Common Questions

**Q: How much does this cost to run?**
A: Costs include Twilio per-minute charges (~$0.013/min) and OpenAI API usage (~$0.002/1K tokens)

**Q: Can I use different AI models?**
A: Yes, modify `OpenAIService.cs` to change the model or use different providers

**Q: How do I handle multiple concurrent calls?**
A: The current architecture supports multiple calls with separate sessions

**Q: Can I add custom commands or keywords?**
A: Yes, add logic in `CallController.Process()` to detect and handle specific phrases

### Troubleshooting

For issues:
1. Check application logs in console/app.log
2. Verify Twilio webhook configuration
3. Test API keys with direct calls
4. Ensure ngrok tunnel is active

## 📝 License

This is a proof-of-concept application. Customize as needed for your use case.

---

**Built with ❤️ using ASP.NET Core 9, Twilio, and OpenAI**