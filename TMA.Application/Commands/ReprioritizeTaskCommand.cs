using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Domain.VOs;
using TMA.Shared;

namespace TMA.Application.Commands;

public class ReprioritizeTaskCommand(TaskId id, TaskPriority priority) : ICommand<TaskDto>
{
    public TaskId Id { get; } = id;
    public TaskPriority Priority { get; } = priority;
}