using TMA.Domain;
using TMA.Domain.VOs;

namespace TMA.Application;

public interface ITaskRepository
{
    public Task AddTaskAsync(TaskEntity taskEntity);
    public Task UpdateTaskAsync(TaskEntity taskEntity);
    public Task<TaskEntity?> GetByIdAsync(TaskId id);
    public IQueryable<TaskEntity?> GetAllAsync();
}