using TMA.Domain.VOs;

namespace TMA.Application.Commands;

public record RenameTaskCommand(TaskId Id, TaskTitle NewTitle);