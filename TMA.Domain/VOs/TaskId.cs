namespace TMA.Domain.VOs;


public class TaskId
{
    public Guid Value { get; init; }

    public TaskId()
    {
        Value = Guid.NewGuid();   
    }

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj)
    {
        if (obj is not TaskId other)
        {
            return false;
        }
        return Value.Equals(other.Value);
    }
    public override int GetHashCode() => Value.GetHashCode();
}