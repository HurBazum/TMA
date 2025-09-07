namespace TMA.Domain.VOs;

public class TaskTitle
{
    public string Value { get; } = null!;

    public TaskTitle(string value)
    {
        if(string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException($"value cannot be empty");
        }

        value = value.Trim();

        if(value.Length > 100 ||  value.Length < 3)
        {
            throw new ArgumentException("value must be 3-100 characters");
        }

        Value = value;
    }

    public override string ToString() => Value;
}