using TMA.Application.MediatorFolder;
using TMA.Domain.VOs;
using TMA.Application.Dtos;

namespace TMA.Application.Commands;

public class RenameTaskCommand(TaskId id, TaskTitle newTitle) : ICommand<TaskDto>
{
    public TaskId Id { get; } = id;
    public TaskTitle NewTitle { get; } = newTitle;
}