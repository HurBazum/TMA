using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using TMA.Application.CommandHandlers;
using TMA.Application.Commands;
using TMA.Application.QueryHandlers;
using TMA.Shared;

namespace TMA.Infrastructure;

public class TaskService(AddTaskCommandHandler addHandler,
                        CompleteTaskCommandHandler completeHandler,
                        RenameTaskCommandHandler renameHandler,
                        ReprioritizeTaskCommandHandler reprioritizeHandler,
                        RescheduleTaskCommandHandler rescheduleHandlere,
                        GetTasksQueryHandler allQuery,
                        GetByIdQueryHandler idQueryHandler,
                        FilterTaskQueryHandler filterHandler) : ITaskService<TaskDto>
{
    private readonly AddTaskCommandHandler _addHandler = addHandler;
    private readonly CompleteTaskCommandHandler _completeHandler = completeHandler;
    private readonly RenameTaskCommandHandler _renameHandler = renameHandler;
    private readonly ReprioritizeTaskCommandHandler _reprioritizeHandler = reprioritizeHandler;
    private readonly RescheduleTaskCommandHandler _rescheduleHandler = rescheduleHandlere;
    private readonly GetTasksQueryHandler _allQueryHandler = allQuery;
    private readonly GetByIdQueryHandler _idQueryHandler = idQueryHandler;
    private readonly FilterTaskQueryHandler _filterHandler = filterHandler;

    public async Task<BaseResponse<TaskDto>> AddAsync(TaskDto dto)
    {
        BaseResponse<TaskDto> response = new();
        
        AddTaskCommand atc = new(new(dto.Title), dto.DeadlineDate, dto.Priority);

        var id = await _addHandler.Handle(atc);

        var newTask = await _idQueryHandler.Handle(new(id));

        response.Value = new()
        {
            Id = newTask.Id.Value.ToString(),
            Title = newTask.Title.Value,
            
            CreatedDate = newTask.CreatedDate,
            Completed = (newTask.Status == Shared.TaskStatus.Completed)
        };

        response.Message = $"Таска успешно добавлена";

        return response;
    }

    public async Task<BaseResponse<List<TaskDto>>> GetAllAsync()
    {
        BaseResponse<List<TaskDto>> response = new();

        var query = _allQueryHandler.Handle();

        var tasks = await query.ToListAsync();

        response.Value = [];

        foreach(var task in tasks)
        {
            response.Value.Add(new() 
            { 
                Id = task.Id.Value.ToString(),
                Title = task.Title.Value, 
                Priority = task.Priority,
                Completed = (task.Status == Shared.TaskStatus.Completed),
                CreatedDate = task.CreatedDate, 
                DeadlineDate = task.Deadline
            });
        }

        return response;
    }

    public async Task<BaseResponse<TaskDto>> UpdateAsync(TaskDto dto)
    {
        BaseResponse<TaskDto> response = new();

        var originTask = await _idQueryHandler.Handle(new(new() { Value = Guid.Parse(dto.Id) }));

        if(originTask.Status == Shared.TaskStatus.Pending && dto.Completed == true)
        {
            await _completeHandler.Handle(new(originTask.Id));
        }
        else
        {
            if(!Equals(originTask.Title.Value, dto.Title))
            {
                await _renameHandler.Handle(new(originTask.Id, new(dto.Title)));
            }
            if(originTask.Deadline != dto.DeadlineDate)
            {
                await _rescheduleHandler.Handle(new(originTask.Id, dto.DeadlineDate));
            }
            if(originTask.Priority != dto.Priority)
            {
                await _reprioritizeHandler.Handle(new(originTask.Id, dto.Priority));
            }
        }

        response.Value = dto;
        response.Message = $"Задача успешно обновлена";

        return response;
    }

    public async Task<BaseResponse<List<TaskDto>>> FilterTaskAsync(FilterDto dto)
    {
        BaseResponse<List<TaskDto>> response = new();

        response.Value = [];

        var tasks = _filterHandler.Handle(new(dto.Priority, dto.Status));

        foreach(var task in tasks)
        {
            response.Value.Add(new()
            {
                Id = task.Id.Value.ToString(),
                Title = task.Title.Value,
                CreatedDate = task.CreatedDate,
                DeadlineDate = task.Deadline,
                Priority = task.Priority,
                Completed = (task.Status == Shared.TaskStatus.Completed)
            });
        }

        response.Message = $"Task was filtered successfuly";

        return response;
    }


}