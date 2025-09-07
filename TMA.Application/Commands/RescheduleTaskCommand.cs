using TMA.Domain.VOs;

namespace TMA.Application.Commands;

public record RescheduleTaskCommand(TaskId Id, DateTime? NewDeadline);