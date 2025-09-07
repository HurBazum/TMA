using TMA.Domain;

namespace TMA.Application.QueryHandlers;

public class GetTasksQueryHandler(ITaskRepository taskRepository)
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public IQueryable<TaskEntity?> Handle() => _taskRepository.GetAllAsync();
}