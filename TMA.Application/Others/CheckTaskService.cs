using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TMA.Application.Dtos;
using TMA.Application.Services;

namespace TMA.Application.Others;

public class CheckTaskService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CheckTaskService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<BaseResponse<List<TaskDto>>> CheckTasksDeadlineAsync()
    {
        FilterDto filter = new()
        {
            To = DateTime.UtcNow,
            Status = Shared.TaskStatus.Pending
        };

        using IServiceScope scope = _scopeFactory.CreateScope();

        ITaskService<TaskDto> service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();

        BaseResponse<List<TaskDto>> response = await service.FilterTaskAsync(filter);

        if(response.Value is null)
        {
            return BaseResponse<List<TaskDto>>.Failure("");
        }
        else
        {
            foreach(var taskDto in response.Value)
            {
            }
        }
        return new();

        //throw new NotImplementedException();
    }
}