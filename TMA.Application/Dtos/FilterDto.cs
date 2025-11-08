using TMA.Shared;

namespace TMA.Application.Dtos;

public class FilterDto
{
    public TaskPriority? Priority { get; set; }
    public Shared.TaskStatus? Status { get; set; }
    public string? Title { get; set; }

    // in progress
    public DateTime? To { get; set; }
    public DateTime? From { get; set; }

    public override string ToString() => $"{Priority} - {Status} - {Title} - {To} - {From}";
}