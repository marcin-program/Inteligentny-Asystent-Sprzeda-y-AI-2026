namespace PetWorld.Core.Entities;

/// <summary>
/// Represents a single conversation session with the AI agent.
/// Stores the Writer-Critic iteration history for audit and debugging.
/// </summary>
public class ChatSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Original user question</summary>
    public string UserQuestion { get; set; } = string.Empty;

    /// <summary>Final approved answer after Writer-Critic loop</summary>
    public string FinalAnswer { get; set; } = string.Empty;

    /// <summary>Number of Writer-Critic iterations (max 3)</summary>
    public int Iterations { get; set; }

    /// <summary>Detailed logs from Writer and Critic agents</summary>
    public List<AgentLog> Logs { get; set; } = new();
}