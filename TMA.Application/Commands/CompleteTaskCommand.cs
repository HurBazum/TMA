using TMA.Application.MediatorFolder;
using TMA.Application.Dtos;
using TMA.Domain.VOs;

namespace TMA.Application.Commands;

public class CompleteTaskCommand(TaskId id) : ICommand<TaskDto>
{
    public TaskId Id { get; } = id;
}