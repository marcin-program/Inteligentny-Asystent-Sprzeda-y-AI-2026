using PetWorld.Application.Interfaces;
using PetWorld.Core.Entities;

namespace PetWorld.Infrastructure.AI;

/// <summary>
/// Fallback service when OpenAI API key is not configured.
/// WHY: Allows application to run in "demo mode" without external dependencies.
/// WHY: Useful for local development, CI/CD pipelines, and testing.
/// </summary>
public class DemoAgentService : IAgentService
{
    public Task<ChatSession> ProcessRequestAsync(string userPrompt)
    {
        var session = new ChatSession
        {
            UserQuestion = userPrompt,
            FinalAnswer = "AI is not configured. Set OPENAI_API_KEY environment variable to enable Writer-Critic AI agent.",
            Iterations = 0
        };

        session.Logs.Add(new AgentLog
        {
            AgentRole = "System",
            Content = "DemoAgentService active (OpenAI key not provided)."
        });

        return Task.FromResult(session);
    }

    public Task<List<ChatSession>> GetHistoryAsync()
        => Task.FromResult(new List<ChatSession>());
}