using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Application.Others;
using TMA.Application.Others.Exceptions;
using TMA.Application.Queries;

namespace TMA.Application.QueryHandlers;

public class GetByIdQueryHandler(ITaskRepository taskRepository) : IQueryHandler<GetByIdQuery, TaskDto>
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public async Task<TaskDto> HandleAsync(GetByIdQuery query)
    {
        var task = await _taskRepository.GetByIdAsync(query.Id)
            ?? throw new TaskWasnotFoundException($"Задача с id={query.Id.Value} не найдена");

        return Transformer.ToDto(task);
    }
}