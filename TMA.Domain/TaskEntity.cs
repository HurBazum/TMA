using TMA.Shared;
using TMA.Domain.VOs;

namespace TMA.Domain;

public class TaskEntity
{
    public TaskId Id { get; init; } = new();
    public TaskTitle Title { get; private set; } = null!;
    public Shared.TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime? Deadline { get; private set; }
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;


    public static TaskEntity Create(TaskTitle _title, DateTime? _deadline, TaskPriority _priority) =>
        new()
        {
            Title = _title,
            Deadline = _deadline,
            Priority = _priority,
            Status = Shared.TaskStatus.Pending
        };

    public void Reprioritize(TaskPriority priority)
    {
        if(priority != Priority)
        {
            Priority = priority;
        }
        else
        {
            return;
        }
    }

    public void Reschedule(DateTime? newDeadline)
    {
        if(newDeadline == null)
        {
            return;
        }
        if(newDeadline >= DateTime.UtcNow)
        {
            Deadline = newDeadline;
        }
    }

    public void Rename(TaskTitle _newTitle) => Title = _newTitle;

    public void Complete()
    {
        if(Status != Shared.TaskStatus.Completed)
        {
            Status = Shared.TaskStatus.Completed;
        }
        else
        {
            return;
        }
    }

    public void Expire()
    {
        if(Status == Shared.TaskStatus.Pending)
        {
            Status = Shared.TaskStatus.Expired;
        }
        else
        {
            return;
        }
    }
}