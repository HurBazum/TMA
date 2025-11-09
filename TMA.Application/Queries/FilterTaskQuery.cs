using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Shared;

namespace TMA.Application.Queries;

public class FilterTaskQuery(TaskPriority? priority = null,
                            Shared.TaskStatus? status = null, 
                            string? title = null, 
                            DateTime? to = null, 
                            DateTime? from = null) : IQuery<List<TaskDto>>
{
    public TaskPriority? Priority { get; init; } = priority;
    public Shared.TaskStatus? Status {  get; init; } = status;
    public string? Title { get; init; } = title;
    public DateTime? To { get; init; } = to;
    public DateTime? From { get; init; } = from;
}