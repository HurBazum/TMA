namespace TMA.Infrastructure;

public class BaseResponse<T> where T : class, new()
{
    public string Message { get; set; } = null!;
    public T? Value { get; set; }
    public List<string> Errors { get; set; } = [];
}