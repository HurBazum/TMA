using TMA.Shared;

namespace TMA.Infrastructure;

public class TaskDto
{
    public string? Id { get; init; }
    public string Title { get; set; } = null!;
    public bool Completed { get; set; }
    public DateTime CreatedDate { get; init; }
    public DateTime? DeadlineDate { get; set; }
    public TaskPriority Priority { get; set; }
    public Dictionary<string, bool> Updates { get; set; } = new();
}