using Microsoft.EntityFrameworkCore;
using TMA.Domain;
using TMA.Shared;

namespace TMA.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<TaskEntity> Tasks { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskEntity>().HasKey(i => i.Id);

        modelBuilder
            .Entity<TaskEntity>()
            .Property(e => e.Id)
            .HasConversion(
                i => i.Value.ToString(),
                i => new() { Value = Guid.Parse(i) });

        modelBuilder
            .Entity<TaskEntity>()
            .Property(e => e.Title)
            .HasConversion(
                t => t.Value,
                t => new(t));

        modelBuilder
            .Entity<TaskEntity>()
            .Property(e => e.Status)
            .HasConversion(
                s => s.ToString(),
                s => Enum.Parse<Shared.TaskStatus>(s));

        modelBuilder
            .Entity<TaskEntity>()
            .Property(e => e.Priority)
            .HasConversion(
                p => p.ToString(),
                p => Enum.Parse<TaskPriority>(p));
    }
}