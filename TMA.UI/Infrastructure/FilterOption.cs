namespace TMA.UI.Infrastructure;

public class FilterOption(string propertyName, string value)
{
    
    public string Value { get; init; } = value;
    public string CommandParameter { get; init; } = $"{propertyName}_{value}";
}