using TMA.Application.Others;
using TMA.Application.Dtos;

namespace TMA.Application.Services;

public interface ITaskService<T> where T : class, new()
{
    public Task<BaseResponse<T>> AddAsync(T item);
    public Task<BaseResponse<List<T>>> GetAllAsync();
    public Task<BaseResponse<T>> UpdateAsync(T item);
    public Task<BaseResponse<List<T>>> FilterTaskAsync(FilterDto item);
}