using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Application.Queries;

namespace TMA.Application.QueryHandlers;

public class GetByTitleQueryHandler(ITaskRepository taskRepository) : IQueryHandler<GetByTitleQuery, TaskDto>
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public Task<TaskDto> HandleAsync(GetByTitleQuery query)
    {
        throw new NotImplementedException();
    }
}