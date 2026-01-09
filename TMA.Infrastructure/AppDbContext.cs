using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TMA.Domain;
using TMA.Domain.VOs;
using TMA.Shared;

namespace TMA.Infrastructure;

public class IdConverter : ValueConverter<TaskId, string>
{
    public IdConverter() : base(
        i => i.Value.ToString(),
        i => new() { Value = Guid.Parse(i) })
    {
    }
}

public class IdComparer : ValueComparer<TaskId>
{
    public IdComparer() : base(
        (i1, i2) => i1.Equals(i2),
        i => i.GetHashCode(),
        i => new TaskId()
        {
            Value = i.Value
        })
    {
    }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskEntity> Tasks { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<TaskId>(i => i.HaveConversion<IdConverter, IdComparer>());
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskEntity>().HasKey(i => i.Id);

        modelBuilder
            .Entity<TaskEntity>()
            .Property(e => e.Title)
            .HasConversion(
                t => t.Value,
                t => new(t));

        

        modelBuilder
            .Entity<TaskEntity>().OwnsOne(e => e.Status, status =>
            {

                status.Property<TaskId>("TaskEntityId").HasConversion<IdConverter, IdComparer>();

                status.WithOwner().HasForeignKey("TaskEntityId");

                status.Property(s => s.Type)
                    .HasColumnName("Status_Type").IsRequired().HasConversion
                    (
                        t => t.ToString(), 
                        t => Enum.Parse<Shared.TaskStatus>(t));

                status.Property(s => s.ChangedAt).IsRequired()
                    .HasColumnName("Status_ChangedAt");
            });


        modelBuilder
            .Entity<TaskEntity>()
            .Property(e => e.Priority)
            .HasConversion(
                p => p.ToString(),
                p => Enum.Parse<TaskPriority>(p));

        modelBuilder.Entity<TaskEntity>().Navigation(e => e.Status).IsRequired();
    }
}