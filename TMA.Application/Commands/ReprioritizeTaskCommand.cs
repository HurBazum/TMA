using TMA.Domain.VOs;
using TMA.Shared;

namespace TMA.Application.Commands;

public record ReprioritizeTaskCommand(TaskId Id, TaskPriority Priority);