using TMA.Domain.VOs;
using TMA.Shared;

namespace TMA.Application.Others;

internal class DomainFilterTaskQuery
{
    public TaskTitle? Title { get; set; }
    public TaskPriority? Priority { get; set; }
    public Shared.TaskStatus? Status { get; set; }

    public DateTime? To { get; set; }
    public DateTime? From { get; set; }
}