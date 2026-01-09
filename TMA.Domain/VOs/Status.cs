using Microsoft.EntityFrameworkCore;
using TMA.Shared;

namespace TMA.Domain.VOs;

[Owned]
public class Status
{
    public Shared.TaskStatus Type { get; private set; }
    public DateTime ChangedAt { get; private set; }

    public static Status Create(Shared.TaskStatus type, DateTime date) => new()
    {
        Type = type,
        ChangedAt = date
    };
    private Status()
    {
        
    }

    public Status Change(Shared.TaskStatus newType)
    {
        if(newType == Type)
        {
            return this;
        }
        else
        {
            return Create(newType, DateTime.UtcNow);
        }
    }

    public override string ToString() => $"{Type} at {ChangedAt}";
}