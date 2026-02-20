using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PetWorld.Application.Interfaces;
using PetWorld.Core.Entities;
using PetWorld.Infrastructure.Data;

namespace PetWorld.Infrastructure.AI;

/// <summary>
/// Critic's response schema for deterministic parsing.
/// WHY JSON: Avoids natural language parsing ambiguity.
/// </summary>
public class CriticFeedback
{
    public bool Approved { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

/// <summary>
/// Writer-Critic AI Agent implementation using Microsoft Semantic Kernel.
///
/// PATTERN: Writer-Critic Loop (Hallucination Prevention)
/// - Writer: Generates customer-facing response
/// - Critic: Validates response against product catalog (grounding)
/// - Loop: Max 3 iterations until Critic approves
///
/// WHY SEMANTIC KERNEL:
/// - Enterprise-grade AI orchestration framework
/// - Built-in chat history management
/// - Plugin architecture for future extensibility
/// - Native .NET integration (vs. Python-first frameworks)
/// </summary>
public class SemanticKernelAgent : IAgentService
{
    private readonly IChatCompletionService _chat;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public SemanticKernelAgent(Kernel kernel, IDbContextFactory<AppDbContext> dbFactory)
    {
        _chat = kernel.GetRequiredService<IChatCompletionService>();
        _dbFactory = dbFactory;
    }

    /// <summary>
    /// Builds product catalog string for Context Injection.
    ///
    /// WHY CONTEXT INJECTION (Simplified RAG):
    /// - LLM has no direct database access
    /// - We "ground" the model by injecting facts into the prompt
    /// - Prevents hallucinations (making up products/prices)
    ///
    /// PRODUCTION CONSIDERATION:
    /// - For large catalogs (>1000 products), use vector search (RAG)
    /// - Current approach works well for small-medium catalogs
    /// </summary>
    private string BuildCatalogString(AppDbContext db)
    {
        var sb = new StringBuilder("PRODUCT CATALOG (Source of Truth):\n");

        foreach (var p in db.Products.AsNoTracking()
                                      .OrderBy(p => p.Category)
                                      .ThenBy(p => p.Name)
                                      .ToList())
        {
            sb.AppendLine($"- {p.Name} | {p.Price:0.00} PLN | {p.Category}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Safely extracts JSON object from LLM response.
    /// WHY: LLMs sometimes wrap JSON in markdown or add explanatory text.
    /// </summary>
    private static string ExtractJsonObject(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "{}";

        var cleaned = input
            .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
            .Replace("```", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        var start = cleaned.IndexOf('{');
        var end = cleaned.LastIndexOf('}');

        if (start >= 0 && end > start)
            return cleaned.Substring(start, end - start + 1);

        return "{}";
    }

    /// <summary>
    /// Main Writer-Critic loop implementation.
    ///
    /// ALGORITHM:
    /// 1. Writer generates response based on catalog context
    /// 2. Critic validates response (checks for hallucinations)
    /// 3. If rejected, Writer gets feedback and tries again (max 3 iterations)
    /// 4. Final answer is persisted to database with full audit trail
    ///
    /// WHY MAX 3 ITERATIONS:
    /// - Balance between quality and latency
    /// - Prevents infinite loops
    /// - Industry standard for self-correction patterns
    /// </summary>
    public async Task<ChatSession> ProcessRequestAsync(string userPrompt)
    {
        using var db = _dbFactory.CreateDbContext();

        var session = new ChatSession
        {
            UserQuestion = userPrompt
        };

        string lastResponse = string.Empty;

        // CRITICAL: Build catalog context from database
        // This is the "grounding" that prevents hallucinations
        var catalogContext = BuildCatalogString(db);

        // WRITER-CRITIC LOOP (max 3 iterations)
        for (int i = 1; i <= 3; i++)
        {
            session.Iterations = i;

            // ===== WRITER AGENT =====
            var writerHistory = new ChatHistory();

            // System prompt with catalog injection
            writerHistory.AddSystemMessage(
                "You are a helpful sales assistant for PetWorld pet store. " +
                "Answer customer questions concisely and professionally. " +
                "CRITICAL: Use ONLY products from the catalog below. Never invent products or prices.\n\n" +
                catalogContext
            );

            // Check if Critic rejected previous attempt
            var lastCriticLog = session.Logs.LastOrDefault(l => l.AgentRole == "Critic");
            if (lastCriticLog != null)
            {
                // Writer gets feedback to improve response
                writerHistory.AddUserMessage(
                    $"CUSTOMER QUESTION:\n{userPrompt}\n\n" +
                    $"YOUR PREVIOUS ANSWER (REJECTED):\n{lastResponse}\n\n" +
                    $"CRITIC FEEDBACK:\n{lastCriticLog.Content}\n\n" +
                    "Please provide a corrected answer addressing the feedback."
                );
            }
            else
            {
                // First iteration - fresh question
                writerHistory.AddUserMessage(userPrompt);
            }

            try
            {
                var response = await _chat.GetChatMessageContentAsync(writerHistory);
                lastResponse = response.Content ?? string.Empty;

                session.Logs.Add(new AgentLog
                {
                    AgentRole = "Writer",
                    Content = lastResponse
                });
            }
            catch (Exception ex)
            {
                lastResponse = $"Error generating response: {ex.Message}";
                break;
            }

            // ===== CRITIC AGENT =====
            var criticHistory = new ChatHistory();

            // CRITICAL: Force JSON output for deterministic parsing
            // WHY: Allows programmatic decision-making (approve/reject)
            criticHistory.AddSystemMessage(
                "You are an Auditor verifying sales assistant responses. " +
                "Check if the answer is factually grounded in the product catalog. " +
                "Verify product names, prices, and availability are correct. " +
                "Return ONLY valid JSON: {\"Approved\": true/false, \"Feedback\": \"reason\"}. " +
                "No markdown, no explanations outside JSON."
            );

            criticHistory.AddUserMessage(
                $"PRODUCT CATALOG:\n{catalogContext}\n\n" +
                $"CUSTOMER QUESTION:\n{userPrompt}\n\n" +
                $"ASSISTANT ANSWER TO VERIFY:\n{lastResponse}"
            );

            try
            {
                var criticMsg = await _chat.GetChatMessageContentAsync(criticHistory);
                var json = ExtractJsonObject(criticMsg.Content ?? "{}");

                session.Logs.Add(new AgentLog
                {
                    AgentRole = "Critic",
                    Content = json
                });

                var feedback = JsonSerializer.Deserialize<CriticFeedback>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // If Critic approves, exit loop early
                if (feedback != null && feedback.Approved)
                {
                    Console.WriteLine($"[Agent] Critic approved on iteration {i}");
                    break;
                }

                Console.WriteLine($"[Agent] Critic rejected iteration {i}: {feedback?.Feedback}");
            }
            catch (Exception ex)
            {
                // If Critic fails (e.g., invalid JSON), accept Writer's answer
                Console.WriteLine($"[Agent] Critic parsing failed: {ex.Message}. Accepting Writer's answer.");
                break;
            }
        }

        session.FinalAnswer = lastResponse;

        // Persist session with full audit trail
        db.ChatSessions.Add(session);
        await db.SaveChangesAsync();

        return session;
    }

    public async Task<List<ChatSession>> GetHistoryAsync()
    {
        using var db = _dbFactory.CreateDbContext();
        return await db.ChatSessions
            .Include(s => s.Logs)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }
}