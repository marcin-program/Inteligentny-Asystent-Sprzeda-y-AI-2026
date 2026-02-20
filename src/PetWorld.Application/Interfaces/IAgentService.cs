using PetWorld.Core.Entities;

namespace PetWorld.Application.Interfaces;

/// <summary>
/// Contract for AI Agent service.
/// Abstracts the implementation details (Semantic Kernel, LangChain, etc.)
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Processes user request through Writer-Critic loop.
    /// Returns ChatSession with final answer and iteration logs.
    /// </summary>
    Task<ChatSession> ProcessRequestAsync(string userPrompt);

    /// <summary>
    /// Retrieves conversation history for audit/analytics.
    /// </summary>
    Task<List<ChatSession>> GetHistoryAsync();
}