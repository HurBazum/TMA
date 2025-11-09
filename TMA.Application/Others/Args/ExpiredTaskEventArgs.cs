using TMA.Application.Dtos;

namespace TMA.Application.Others.Args;

public class ExpiredTaskEventArgs : EventArgs
{
    public string DtoId { get; set; } = null!;
    public string Message { get; set; } = null!;
}