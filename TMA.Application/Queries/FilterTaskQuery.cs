using TMA.Shared;

namespace TMA.Application.Queries;

public record FilterTaskQuery(TaskPriority? Priority = null, Shared.TaskStatus? Status = null, string? Title = null, DateTime? To = null, DateTime? From = null);