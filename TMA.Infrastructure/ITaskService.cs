namespace TMA.Infrastructure;

public interface ITaskService<T> where T : class, new()
{
    public Task<BaseResponse<T>> AddAsync(T item);
    public Task<BaseResponse<List<T>>> GetAllAsync();
    public Task<BaseResponse<T>> UpdateAsync(T item);
    public Task<BaseResponse<List<T>>> FilterTaskAsync(FilterDto item);
}