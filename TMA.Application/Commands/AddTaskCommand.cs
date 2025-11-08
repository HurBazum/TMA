using TMA.Application.MediatorFolder;
using TMA.Application.Dtos;
using TMA.Domain.VOs;
using TMA.Shared;

namespace TMA.Application.Commands;

public class AddTaskCommand(TaskTitle taskTitle, DateTime? deadline, TaskPriority priority) : ICommand<TaskDto>
{
    public TaskTitle Title { get; } = taskTitle;
    public DateTime? Deadline { get; } = deadline;
    public TaskPriority Priority { get; } = priority;
}