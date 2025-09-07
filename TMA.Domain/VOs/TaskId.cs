namespace TMA.Domain.VOs;

public class TaskId
{
    public Guid Value { get; init; }

    public TaskId()
    {
        Value = Guid.NewGuid();   
    }

    public override string ToString() => Value.ToString();
}