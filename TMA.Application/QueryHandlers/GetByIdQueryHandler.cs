using TMA.Application.Queries;
using TMA.Domain;
using TMA.Domain.VOs;

namespace TMA.Application.QueryHandlers;

public class GetByIdQueryHandler(ITaskRepository taskRepository)
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public async Task<TaskEntity> Handle(GetByIdQuery query)
    {
        var task = await _taskRepository.GetByIdAsync(query.Id);

        return task ?? throw new NullReferenceException();
    }
}