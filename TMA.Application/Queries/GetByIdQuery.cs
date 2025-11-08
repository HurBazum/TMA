using TMA.Domain.VOs;
using TMA.Application.MediatorFolder;
using TMA.Application.Dtos;

namespace TMA.Application.Queries;

public record GetByIdQuery : IQuery<TaskDto>
{
    public TaskId Id { get; init; } = null!;
}