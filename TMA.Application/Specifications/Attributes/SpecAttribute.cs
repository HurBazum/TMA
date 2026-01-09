namespace TMA.Application.Specifications.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SpecAttribute : Attribute
{
    public Type Specification { get; init; } = null!;
}