using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Domain.VOs;

namespace TMA.Application.Commands;

public class RescheduleTaskCommand(TaskId id, DateTime? newDeadline) : ICommand<TaskDto>
{
    public TaskId Id { get; } = id;
    public DateTime? NewDeadline { get; } = newDeadline;
}