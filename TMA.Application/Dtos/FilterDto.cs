using TMA.Application.Dtos.DtoAttributes;
using TMA.Shared;

namespace TMA.Application.Dtos;

public class FilterDto
{
    public TaskPriority? Priority { get; set; }
    public Shared.TaskStatus? Status { get; set; }
    public string? Title { get; set; }

    [Date]
    public DateTime? To { get; set; }
    [Date]
    public DateTime? From { get; set; }

    public override string ToString() => $"{Priority} - {Status} - {Title} - {To} - {From}";
}