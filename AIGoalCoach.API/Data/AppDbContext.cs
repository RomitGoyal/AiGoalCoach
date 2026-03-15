using Microsoft.EntityFrameworkCore;
using AIGoalCoach.API.Models;

namespace AIGoalCoach.API.Data;

public class AppDbContext : DbContext
{
    public DbSet<Goal> Goals { get; set; } = null!;
    
    public DbSet<TelemetryEvent> TelemetryEvents { get; set; } = null!;
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RefinedGoal).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.KeyResults).HasConversion(
                v => string.Join("||", v),
                v => v.Split("||", StringSplitOptions.RemoveEmptyEntries).ToArray()
            );
        });
    }
}
