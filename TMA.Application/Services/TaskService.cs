using TMA.Application.Commands;
using TMA.Application.Dtos;
using TMA.Application.Others;
using TMA.Application.MediatorFolder;
using TMA.Application.Queries;

namespace TMA.Application.Services;

public class TaskService(IMediator mediator, IUnitOfWork uow) : ITaskService<TaskDto>
{
    private readonly IMediator _mediator = mediator;
    private readonly IUnitOfWork _uow = uow;
    public async Task<BaseResponse<TaskDto>> AddAsync(TaskDto dto)
    {
        try
        {
            AddTaskCommand atc = new(new(dto.Title), dto.DeadlineDate, dto.Priority);

            TaskDto result = await _mediator.SendASync(atc);

            await _uow.SaveAsync();

            return BaseResponse<TaskDto>.Success(result, $"Таска успешно добавлена");

        }
        catch(Exception ex)
        {
            return BaseResponse<TaskDto>.Failure(ex.Message);
        }
    }

    public async Task<BaseResponse<List<TaskDto>>> GetAllAsync()
    {
        try
        {
            var result = await _mediator.SendASync(new GetTasksQuery());

            return BaseResponse<List<TaskDto>>.Success(result, $"Таски успешно получены");
        }
        catch(Exception ex)
        {
            return BaseResponse<List<TaskDto>>.Failure(ex.Message);
        }
    }

    public async Task<BaseResponse<TaskDto>> UpdateAsync(TaskDto dto)
    {
        try
        {
            GetByIdQuery idQuery = new()
            {
                Id = new() { Value = Guid.Parse(dto.Id) }
            };

            var originTask = await _mediator.SendASync(idQuery);

            var id = idQuery.Id;

            if(originTask.Completed == false && dto.Completed == true)
            {
                await _mediator.SendASync(new CompleteTaskCommand(id));
            }
            else
            {
                if(!Equals(originTask.Title, dto.Title))
                {
                    await _mediator.SendASync(new RenameTaskCommand(id, new(dto.Title)));
                }
                if(originTask.DeadlineDate != dto.DeadlineDate)
                {
                    await _mediator.SendASync(new RescheduleTaskCommand(id, dto.DeadlineDate));
                }
                if(originTask.Priority != dto.Priority)
                {
                    await _mediator.SendASync(new ReprioritizeTaskCommand(id, dto.Priority));
                }
            }

            await _uow.SaveAsync();

            return BaseResponse<TaskDto>.Success(dto, $"Задача успешно обновлена");
        }
        catch(Exception ex)
        {
            return BaseResponse<TaskDto>.Failure(ex.Message);
        }
    }

    public async Task<BaseResponse<List<TaskDto>>> FilterTaskAsync(FilterDto dto)
    {
        try
        {
            // priority, status, title, to, from
            var tasks = await _mediator.SendASync(new FilterTaskQuery(dto.Priority, dto.Status, dto.Title));

            return BaseResponse<List<TaskDto>>.Success(tasks, $"Task was filtered successfuly");
        }
        catch(Exception ex)
        {
            return BaseResponse<List<TaskDto>>.Failure(ex.Message);
        }
    }
}