### 🧱 Phase 1: Foundation Setup ✅ **COMPLETED**

**User Story 1.1**  
“As a developer, I want to create a new .NET 9 Web API project so that I have a foundation to build my app.”  
- Task: Run dotnet CLI to scaffold the project  
- Task: Set up project file, solution, and folder structure  

**User Story 1.2**  
“As a developer, I want to add the Twilio NuGet package so that I can use the Twilio Voice API in my app.”  
- Task: Install latest official Twilio NuGet package  
- Task: Confirm install and usage with a HelloWorld Twilio class  

**User Story 1.3**  
“As a developer, I want to store Twilio secrets securely so that my credentials aren’t hard-coded.”  
- Task: Add secrets to appsettings.Development.json or dotnet secrets  
- Task: Create a helper class to load Twilio config values  

---

### 📞 Phase 2: Twilio Voice Integration ✅ **COMPLETED**

**User Story 2.1**  
“As a caller, I want to call a phone number and have it connect to my app so I can begin a voice conversation.”  
- Task: Create an API controller with POST /api/call/answer  
- Task: Return TwiML with a basic <Say> or <Gather> prompt  

**User Story 2.2**  
“As a user, I want the system to capture what I say using speech-to-text so that the AI can understand me.”  
- Task: Configure <Gather input="speech">  
- Task: Set callback endpoint for speech event  

**User Story 2.3**  
“As a developer, I want to track calls by Call SID so I can maintain conversation context.”  
- Task: Create an in-memory session store for active calls  
- Task: Add basic logging of Call SID and From/To numbers  

---

### 🤖 Phase 3: OpenAI Integration ✅ **COMPLETED**

**User Story 3.1**  
“As a user, I want my speech-to-text input sent to the LLM so that it can respond intelligently.”  
- Task: Receive STT text via /api/call/process  
- Task: Send user text input to OpenAI or OpenAI-compatible API (via HTTP)  

**User Story 3.2**  
“As a developer, I want the system message (prompt) to guide LLM behavior to avoid unwanted responses.”  
- Task: Define simple system prompt  
- Task: Build prompt + user message as request payload  

**User Story 3.3**  
“As the Assistant, I want to take the AI response and prepare it for voice output.”  
- Task: Parse LLM response  
- Task: Store or return trimmed response text for TwiML rendering  

---

### 🗣️ Phase 4: Voice Output & Conversation Loop ✅ **COMPLETED**

**User Story 4.1**  
“As a caller, I want the AI's response to be spoken aloud to me so that I can hear the answer.”  
- Task: Insert <Say> tag in TwiML response with LLM text  
- Optional Task: Explore using Twilio <Play> if using TTS mp3 from OpenAI  

**User Story 4.2**  
“As the system, I want to prompt the user again after responding so that the conversation keeps going.”  
- Task: Add another <Gather> block at the end of the response  
- Task: Send it to same or new /api/call/process loop

**User Story 4.3**  
“As a developer, I want to cleanly format all TwiML interactions so they’re readable and debuggable.”  
- Task: Use Twilio SDK to dynamically generate TwiML XML  
- Task: Add logging or return fallback message on errors  

---

### 🧪 Phase 5: Testing & Debugging 🔄 **IN PROGRESS**

**User Story 5.1**  
“As a developer, I want to test the app locally with ngrok so that I can simulate real Twilio phone calls.”  
- Task: Run ngrok and expose localhost (→ twilio webhook)  
- Task: Configure Twilio Phone Number → voice webhook to ngrok URL  

**User Story 5.2**  
“As a user, I want to make a test phone call to the number so I can experience the full conversation flow.”  
- Task: Call Twilio number & confirm voice round-trip  
- Task: Log conversation history in local console for debugging  

**User Story 5.3**  
“As a developer, I want to verify that speech recognition is accurate and responses are fast.”  
- Task: Test different speech inputs  
- Task: Monitor latency for transcription → response  

---

## ✅ Bonus (Stretch Goals, Optional)

- 🔒 Add a simple auth token on webhooks (basic verification only)
- 📝 Store request/responses to a log file for reviewing LLM interaction
- ⚙️ Set up a configuration file for model name, system prompt, etc.

---
