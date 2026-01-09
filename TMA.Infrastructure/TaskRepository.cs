using TMA.Application;
using TMA.Domain;
using TMA.Domain.VOs;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace TMA.Infrastructure;

public class TaskRepository(AppDbContext context) : ITaskRepository
{
    private readonly AppDbContext _context = context;
    public async Task<TaskEntity> AddTaskAsync(TaskEntity taskEntity)
    {
        var entry = _context.Entry(taskEntity);

        _context.Tasks.Add(taskEntity);

        //var r = entry.Reference(t => t.Status).TargetEntry.Property("TaskEntityId").CurrentValue;
        //var debug = context.ChangeTracker.DebugView.LongView;
        //entry.State = EntityState.Added;

        return await Task.FromResult(taskEntity);
    }

    public IQueryable<TaskEntity?> GetAllAsync() => _context.Tasks.AsNoTracking();

    public async Task<TaskEntity?> GetByIdAsync(TaskId id) => await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<TaskEntity> UpdateTaskAsync(TaskEntity taskEntity)
    {
        var entry = _context.Entry(taskEntity);
        entry.State = EntityState.Modified;

        return await Task.FromResult(taskEntity);
    }
}