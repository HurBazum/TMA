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
    public EventHandler<ExpiredTaskEventArgs>? ExpiredTask;

    public CheckTaskService(IServiceScopeFactory scopeFactory, IMediator mediator)
    {
        _scopeFactory = scopeFactory;
        _mediator = mediator;
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

            List<TaskDto> readyToExpiring = await _mediator.SendASync(ftq);

            if(readyToExpiring is null)
            {
                return;
            }
            else
            {
                foreach(var taskDto in readyToExpiring)
                {
                    if(taskDto.DeadlineDate is not null)
                    {
                        TaskId id = new()
                        {
                            Value = Guid.Parse(taskDto.Id)
                        };

                        ExpireTaskCommand etc = new(id);

                        TaskDto expiredDto = await _mediator.SendASync(etc);

                        if(expiredDto is not null)
                        {
                            OnExpiredTask(expiredDto.Id);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            await Task.Delay(60000, token);
        }
    }

    public void OnExpiredTask(string id) => ExpiredTask?.Invoke(this, new()
    {
        DtoId = id,
        Message = $"Task {id} was expired"
    });
}