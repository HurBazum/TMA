namespace TMA.Application.Others;

public class BaseResponse<T> where T : class, new()
{
    public string Message { get; init; } = null!;
    public T? Value { get; init; }

    public static BaseResponse<T> Success(T value, string message) => new()
    {
        Message = message,
        Value = value
    };

    public static BaseResponse<T> Failure(string message) => new()
    {
        Message = message
    };
}