using TMA.Application.MediatorFolder;
using TMA.Application.Queries;
using TMA.Application.Dtos;
using TMA.Application.Others;

namespace TMA.Application.QueryHandlers;

public class GetByIdQueryHandler(ITaskRepository taskRepository) : IQueryHandler<GetByIdQuery, TaskDto>
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public async Task<TaskDto> HandleAsync(GetByIdQuery query)
    {
        var task = await _taskRepository.GetByIdAsync(query.Id) ?? throw new Exception();
        
        return Transformer.ToDto(task);
    }
}