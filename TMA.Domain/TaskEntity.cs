using TMA.Shared;
using TMA.Domain.VOs;
using System.ComponentModel.DataAnnotations;

namespace TMA.Domain;

public class TaskEntity
{
    public TaskId Id { get; init; } = null!;
    public TaskTitle Title { get; private set; } = null!;
    public TaskPriority Priority { get; private set; }
    public DateTime? Deadline { get; private set; }
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Required]
    public Status Status { get; private set; } = null!;

    public static TaskEntity Create(TaskTitle _title, DateTime? _deadline, TaskPriority _priority) =>
        new()
        {
            Id = new TaskId() { Value = Guid.NewGuid() },
            Title = _title,
            Deadline = _deadline,
            Priority = _priority,
            Status = Status.Create(Shared.TaskStatus.Pending, DateTime.UtcNow)
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
        if(Status.Type == Shared.TaskStatus.InProgress)
        {
            Status = Status.Change(Shared.TaskStatus.Completed);
        }
        else
        {
            return;
        }
    }

    public void Expire()
    {
        if(Status.Type == Shared.TaskStatus.InProgress && Deadline <= DateTime.UtcNow)
        {
            Status = Status.Change(Shared.TaskStatus.Expired);
        }
        else
        {
            return;
        }
    }
}