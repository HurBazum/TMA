using TMA.Application.Specifications;
using TMA.Application.Specifications.Attributes;
using TMA.Shared;

namespace TMA.Application.Others;

public class DomainFilterTaskQuery
{
    [Spec(Specification = typeof(TaskTitleSpecification))]
    public string? Title { get; set; }

    [Spec(Specification = typeof(TaskPrioritySpecification))]
    public TaskPriority? Priority { get; set; }

    [Spec(Specification = typeof(TaskStatusSpecification))]
    public Shared.TaskStatus? Status { get; set; }

    [Spec(Specification = typeof(TaskDeadlineSpecification))]
    public DateTime? To { get; set; }

    [Spec(Specification = typeof(TaskCreatedDateSpecification))]
    public DateTime? From { get; set; }
}