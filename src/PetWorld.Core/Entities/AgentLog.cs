namespace PetWorld.Core.Entities;

/// <summary>
/// Individual log entry from Writer or Critic agent.
/// Used for transparency and debugging of AI decision-making process.
/// </summary>
public class AgentLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChatSessionId { get; set; }

    /// <summary>Agent role: "Writer", "Critic", or "System"</summary>
    public string AgentRole { get; set; } = string.Empty;

    /// <summary>Agent's output or feedback</summary>
    public string Content { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}