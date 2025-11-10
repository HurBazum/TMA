using Microsoft.Extensions.DependencyInjection;
using TMA.Application.Commands;
using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Application.Others.Args;
using TMA.Application.Queries;
using TMA.Domain.VOs;

namespace TMA.Application.Services;

public class CheckTaskService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    public EventHandler<ExpiredTaskEventArgs>? ExpiredTask;

    public CheckTaskService(IServiceScopeFactory scopeFactory, IMediator mediator, IUnitOfWork unitOfWork)
    {
        _scopeFactory = scopeFactory;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    public async Task CheckTasksDeadlineAsync(CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();

            FilterTaskQuery ftq = new()
            {
                Status = Shared.TaskStatus.Pending,
                To = DateTime.UtcNow
            };

            List<TaskDto> readyToExpiring = await _mediator.SendAsync(ftq);

            if(readyToExpiring is null || readyToExpiring.Count == 0)
            {
                await Task.Delay(60000, token);
                continue;
            }
            else
            {
                foreach(TaskDto taskDto in readyToExpiring)
                {
                    TaskId id = new()
                    {
                        Value = Guid.Parse(taskDto.Id)
                    };

                    ExpireTaskCommand etc = new(id);

                    TaskDto expiredDto = await _mediator.SendAsync(etc);

                    await _unitOfWork.SaveAsync();

                    OnExpiredTask(expiredDto.Id);                    
                }
            }
        }
    }

    public void OnExpiredTask(string id) => ExpiredTask?.Invoke(this, new()
    {
        DtoId = id,
        Message = $"Task {id} was expired"
    });
}