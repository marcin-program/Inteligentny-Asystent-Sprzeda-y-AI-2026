using Microsoft.EntityFrameworkCore;
using PetWorld.Core.Entities;

namespace PetWorld.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context.
/// Manages ChatSessions, Products, and AgentLogs.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<AgentLog> AgentLogs => Set<AgentLog>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure ChatSession -> AgentLog relationship
        modelBuilder.Entity<ChatSession>()
            .HasMany(s => s.Logs)
            .WithOne()
            .HasForeignKey(l => l.ChatSessionId);

        // Configure decimal precision for MySQL compatibility
        // WHY: MySQL requires explicit precision for DECIMAL columns
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}