using TMA.Application.Dtos;

namespace TMA.UI.Infrastructure.EventArguments;

public class CreateUpdateEventArgs : EventArgs
{
    public string? OperationName { get; set; }
    public TaskDto? Dto { get; set; }
    public string? Message { get; set; }
}