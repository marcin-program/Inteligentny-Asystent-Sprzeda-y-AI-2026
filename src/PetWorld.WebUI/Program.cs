using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using PetWorld.Application.Interfaces;
using PetWorld.Infrastructure.AI;
using PetWorld.Infrastructure.Data;
using PetWorld.WebUI.Components;

var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE CONFIGURATION =====
// WHY IDbContextFactory: Blazor Server uses SignalR (long-lived connections)
// Using regular DbContext causes concurrency issues with multiple users
// Factory pattern creates new context per operation (thread-safe)
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
           ?? "Server=db;Database=petworld;User=root;Password=root;";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

// ===== BLAZOR CONFIGURATION =====
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===== AI CONFIGURATION =====
// Graceful degradation: App works without OpenAI key (demo mode)
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? builder.Configuration["OPENAI_API_KEY"]
            ?? "CHANGE_ME";

bool aiEnabled =
    !string.IsNullOrWhiteSpace(apiKey) &&
    apiKey != "CHANGE_ME" &&
    apiKey != "CHANGE_ME_IN_DOCKER_COMPOSE" &&
    !apiKey.StartsWith("sk-proj-DEMO");

if (aiEnabled)
{
    Console.WriteLine("[Startup] AI enabled with OpenAI GPT-4o");

    // Register Semantic Kernel with OpenAI
    builder.Services.AddKernel()
        .AddOpenAIChatCompletion("gpt-4o", apiKey);

    builder.Services.AddScoped<IAgentService, SemanticKernelAgent>();
}
else
{
    Console.WriteLine("[Startup] AI disabled - using DemoAgentService");
    builder.Services.AddScoped<IAgentService, DemoAgentService>();
}

var app = builder.Build();

// ===== DATABASE INITIALIZATION =====
// WHY: Ensures database exists and is seeded before first request
// PRODUCTION: Use proper migrations instead of EnsureCreated
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = dbFactory.CreateDbContext();

        Console.WriteLine("[Startup] Initializing database...");
        db.Database.EnsureCreated();

        // Load products from CSV (source of truth for AI grounding)
        ProductSeeder.LoadFromCsv(db);

        Console.WriteLine("[Startup] Database ready");
    }
    catch (Exception ex)
    {
        // Non-fatal: Allow app to start even if DB fails (for debugging)
        Console.WriteLine($"[Startup] Database initialization failed: {ex.Message}");
    }
}

// ===== MIDDLEWARE PIPELINE =====
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();