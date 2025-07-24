
**Project Development Roadmap: Twilio + OpenAI Voice Assistant POC**  
**Core Objective:** Enable voice-to-voice conversations via Twilio phone calls with an LLM

---

### **Phase 1: Foundation Setup** ✅ **COMPLETED**
**Objective:** Basic .NET 9 web app infrastructure  

| Task | Time Estimate | Details | Status |
|------|---------------|---------|--------|
| 1.1 Create .NET 9 Web API Project | 2 hrs | `dotnet new webapi`, configure minimal APIs | ✅ |
| 1.2 Add Twilio SDK | 1 hr | `Install-Package Twilio -Version 8.x` *or latest* | ✅ |
| 1.3 Configure App Secrets | 1 hr | Store Twilio credentials (`AccountSid`, `AuthToken`) | ✅ |

---

### **Phase 2: Twilio Voice Integration** ✅ **COMPLETED**
**Objective:** Handle inbound/outbound voice calls  

| Task | Time Estimate | Details | Status |
|------|---------------|---------|--------|
| 2.1 Create Call Endpoint | 3 hrs | `[POST] /api/call/answer` returning TwiML for voice prompts | ✅ |
| 2.2 Speech-to-Text Setup | 4 hrs | Configure `<Gather>` with `input="speech"` | ✅ |
| 2.3 Call Status Tracking | 2 hrs | Simple in-memory storage for call SIDs | ✅ |

---

### **Phase 3: OpenAI Integration** ✅ **COMPLETED**
**Objective:** Connect voice input to LLM  

| Task | Time Estimate | Details | Status |
|------|---------------|---------|--------|
| 3.1 Speech Processing Endpoint | 3 hrs | `[POST] /api/call/process` to receive Twilio voice input | ✅ |
| 3.2 STT → LLM Flow | 4 hrs | Use OpenAI API (or compatible) to process text input | ✅ |
| 3.3 Response Generation | 3 hrs | Format LLM response for voice output | ✅ |

---

### **Phase 4: Voice Synthesis & Delivery** ✅ **COMPLETED**
**Objective:** Convert text responses to voice  

| Task | Time Estimate | Details | Status |
|------|---------------|---------|--------|
| 4.1 TTS Implementation | 6 hrs | Use Twilio's `<Say>` or OpenAI TTS API | ✅ |
| 4.2 Dynamic TwiML Builder | 3 hrs | Generate response TwiML programmatically | ✅ |
| 4.3 Conversation Loop | 3 hrs | Maintain dialogue state with call SID | ✅ |

---

### **Phase 5: Testing & Debugging** 🔄 **IN PROGRESS**

| Task | Time Estimate | Details | Status |
|------|---------------|---------|--------|
| 5.1 Ngrok Setup | 1 hr | Public URL for local endpoint testing | ✅ |
| 5.2 Twilio Webhook Config | 1 hr | Point phone number to ngrok URL | ⏳ |
| 5.3 E2E Testing Scenarios | 4 hrs | Validate complete voice conversation flow | ⏳ |

---

### **Technical Stack**  
- **Backend**: ASP.NET Core 9 Minimal API  
- **AI**: OpenAI API (or local LLM via LiteLLM)  
- **Voice**: Twilio Programmable Voice  
- **Auth**: None required (POC)  
- **Storage**: In-memory (upgrade to Redis later if needed)  

---

### **Risk Mitigation**  
1. Voice Latency: Implement 5-second timeout with retry logic  
2. LLM Hallucinations: Add system prompt constraints  
3. Twilio Call Limits: Monitor via Twilio Console  

---

### **First Action Items**  
1. Create project: `dotnet new webapi -n VoicePOC`
2. Add Twilio SDK: `Install-Package Twilio`
3. Create empty controller: `/api/call/answer`