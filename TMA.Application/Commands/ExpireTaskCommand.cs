using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Domain.VOs;

namespace TMA.Application.Commands;

public class ExpireTaskCommand(TaskId id) : ICommand<TaskDto>
{
    public TaskId Id { get; init; } = id;
}