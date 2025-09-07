using TMA.Domain.VOs;
using TMA.Shared;

namespace TMA.Application.Commands;

public record AddTaskCommand(TaskTitle TaskTitle, DateTime? Deadline, TaskPriority Priority);