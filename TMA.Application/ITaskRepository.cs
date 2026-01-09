using TMA.Domain;
using TMA.Domain.VOs;

namespace TMA.Application;

public interface ITaskRepository
{
    public Task<TaskEntity> AddTaskAsync(TaskEntity taskEntity);
    public Task<TaskEntity> UpdateTaskAsync(TaskEntity taskEntity);
    public Task<TaskEntity?> GetByIdAsync(TaskId id);
    public IQueryable<TaskEntity?> GetAllAsync();
}