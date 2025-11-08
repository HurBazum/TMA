using TMA.Application.Dtos;

namespace TMA.UI.Infrastructure.EventArguments;

public class FilterDoneEventArgs : EventArgs
{
    public string? Message { get; set; }
    public ICollection<TaskDto>? Value { get; set; }
}