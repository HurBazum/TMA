using TMA.Application.Specifications;
using TMA.Application.Specifications.Attributes;

namespace TMA.Application.Dtos;

public class ActiveDto
{
    [Spec(Specification = typeof(TaskStatusSpecification))]
    public Shared.TaskStatus FirstStatus { get; } = Shared.TaskStatus.Pending;


    [Spec(Specification = typeof(TaskStatusSpecification))]
    public Shared.TaskStatus SecondStatus { get; } = Shared.TaskStatus.InProgress;
}