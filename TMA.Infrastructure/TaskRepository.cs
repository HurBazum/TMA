using TMA.Application;
using TMA.Domain;
using TMA.Domain.VOs;
using Microsoft.EntityFrameworkCore;

namespace TMA.Infrastructure;

public class TaskRepository(AppDbContext context) : ITaskRepository
{
    private readonly AppDbContext _context = context;
    public async Task AddTaskAsync(TaskEntity taskEntity)
    {
        var entry = _context.Entry(taskEntity);

        entry.State = EntityState.Added;
        await _context.SaveChangesAsync();
    }

    public IQueryable<TaskEntity?> GetAllAsync() => _context.Tasks.AsNoTracking();

    public async Task<TaskEntity?> GetByIdAsync(TaskId id) => await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public async Task UpdateTaskAsync(TaskEntity taskEntity)
    {
        var entry = _context.Entry(taskEntity);
        entry.State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}