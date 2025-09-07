using TMA.Shared;

namespace TMA.Infrastructure;

public class FilterDto
{
    public TaskPriority? Priority { get; set; }
    public Shared.TaskStatus? Status { get; set; }
    public string? Title { get; set; }

    // not implemented
    public DateTime? To { get; set; }
    public DateTime? From { get; set; }

    public override string ToString() => $"{Priority} - {Status} - {Title} - {To} - {From}";
}