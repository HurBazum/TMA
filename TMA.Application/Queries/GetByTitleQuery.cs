using TMA.Application.MediatorFolder;
using TMA.Application.Dtos;

namespace TMA.Application.Queries;

public class GetByTitleQuery : IQuery<TaskDto>
{
    public string Title { get; init; } = null!;
}