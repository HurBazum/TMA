using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Application.Others;
using TMA.Application.Queries;

namespace TMA.Application.QueryHandlers;

public class GetTasksQueryHandler(ITaskRepository taskRepository) : IQueryHandler<GetTasksQuery, List<TaskDto>>
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public async Task<List<TaskDto>> HandleAsync(GetTasksQuery query)
    {
        var tasks = _taskRepository.GetAllAsync();

        var dtos = Transformer.ToDtos(tasks);
        
        return await Task.FromResult(dtos);
    }
}