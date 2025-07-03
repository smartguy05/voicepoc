using VoicePOC.Models;
using VoicePOC.Services;
using Twilio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure Twilio and OpenAI settings
builder.Services.Configure<TwilioConfig>(builder.Configuration.GetSection("Twilio"));
builder.Services.Configure<OpenAIConfig>(builder.Configuration.GetSection("OpenAI"));

// Add HTTP client for OpenAI API calls with timeout
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(8); // 8 second timeout
});

// Add call session service
builder.Services.AddSingleton<ICallSessionService, CallSessionService>();

// Add OpenAI service
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

// Add OpenAI streaming service
builder.Services.AddHttpClient<IOpenAIStreamingService, OpenAIStreamingService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); // Longer timeout for streaming
});
builder.Services.AddScoped<IOpenAIStreamingService, OpenAIStreamingService>();

// Add TwiML services
builder.Services.AddScoped<ITwiMLService, TwiMLService>();
builder.Services.AddScoped<IStreamingTwiMLService, StreamingTwiMLService>();

// Add streaming session service
builder.Services.AddSingleton<IStreamingSessionService, StreamingSessionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Initialize Twilio client
var twilioConfig = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<TwilioConfig>>().Value;
TwilioClient.Init(twilioConfig.AccountSid, twilioConfig.AuthToken);

app.MapControllers();

app.Run();
