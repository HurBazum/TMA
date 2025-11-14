using System.Windows.Input;
using TMA.Application.Services;
using TMA.Application.Dtos;
using TMA.UI.Infrastructure.Commands;
using TMA.UI.Infrastructure.Commands.Base;
using TMA.UI.Infrastructure.EventArguments;
using TMA.UI.Infrastructure.Stores;
using TMA.UI.ViewModels.Base;
using TMA.Shared;
using Microsoft.Extensions.DependencyInjection;
using TMA.Application.Others;

namespace TMA.UI.ViewModels;

public class CreateUpdateViewModel(IServiceScopeFactory scopeFactory, INavigationStore navigationStore) : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly INavigationStore _navigationStore = navigationStore;

    public event EventHandler<CreateUpdateEventArgs>? OperationDone;
    
    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => Set(ref _message, value);
    }

    public bool IsUpdate => Id != string.Empty;

    private string _id = string.Empty;
    public string Id
    {
        get => _id;
        private set => Set(ref _id, value);
    }

    private string _taskTitle = string.Empty;
    public string TaskTitle
    {
        get => _taskTitle;
        set => Set(ref _taskTitle, value);
    }

    private TaskPriority _priority;
    public TaskPriority Priority
    {
        get => _priority;
        set => Set(ref _priority, value);
    }

    private DateTime? _deadline = DateTime.Today;
    public DateTime? Deadline
    {
        get => _deadline;
        set => Set(ref _deadline, value); 
    }
    private bool _isLimited;
    public bool IsLimited
    {
        get => _isLimited;
        set => Set(ref _isLimited, value);
    }

    private IAsyncCommand AddTaskAsyncCmd => new AsyncCommand(AddTaskCmdExecute, CanAddTaskCmdExecuted);
    private IAsyncCommand UpdateTaskAsyncCmd => new AsyncCommand(UpdateTaskExecuteAsync, CanUpdateTaskExecuteAsync);
    
    private enum DataValue
    {
        Day,
        Month,
        Year,
        Priority
    }

    public ICommand ChangeValueCmd => new LambdaCommand(ChangeValueCmdExecute, CanChangeValueCmdExecute);    
    public CmdAdapter AddTaskCmdAdapt => new(AddTaskAsyncCmd);
    private CmdAdapter UpdateTaskAsyncCmdAdapt => new(UpdateTaskAsyncCmd);

    
    public LambdaCommand AddTaskCmd => new(AddTaskCmdAdapt.Execute, AddTaskCmdAdapt.CanExecute);
    public LambdaCommand UpdateTaskCmd => new(UpdateTaskAsyncCmdAdapt.Execute, UpdateTaskAsyncCmdAdapt.CanExecute);
    public LambdaCommand GoBackCmd => new(p => { _navigationStore.Previous(); }, p => true);

    private bool CanChangeValueCmdExecute(object parameter) => true;
    private void ChangeValueCmdExecute(object parameter)
    {
        if(parameter is not string obj || string.IsNullOrEmpty(obj))
        {
            return;
        }

        string[] propAction = obj.Split('_');

        if(propAction.Length < 2)
        {
            return;
        }

        bool isIncrement = (propAction[1] == "Increment");

        int x = isIncrement ? 1 : -1;

        bool result = Enum.TryParse<DataValue>(propAction[0], out DataValue data);

        if(!result)
        {
            return;
        }

        switch(data)
        {
            case DataValue.Day:
                Deadline = Deadline.Value.AddDays(x);
                break;
            case DataValue.Month:
                Deadline = Deadline.Value.AddMonths(x);
                break;
            case DataValue.Year:
                Deadline = Deadline.Value.AddYears(x);
                break;
            case DataValue.Priority:
                if(isIncrement)
                {
                    Priority = (Priority < TaskPriority.High) ? Priority + 1 : TaskPriority.Normal;
                }
                else
                {
                    Priority = (Priority > TaskPriority.Normal) ? Priority - 1 : TaskPriority.High;
                };
                break;
        }
    }

    private bool CanAddTaskCmdExecuted(object? parameter)
    {
        if(string.IsNullOrEmpty(TaskTitle) || !string.IsNullOrEmpty(Id))
        {
            return false;
        }
        return true;
    }
    private async Task AddTaskCmdExecute(object? parameter)
    {
        TaskDto dto = new() { Title = TaskTitle, Priority = Priority };

        using IServiceScope scope = _scopeFactory.CreateScope();

        ITaskService<TaskDto> service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();

        if(IsLimited)
        {
            dto.DeadlineDate = Deadline;
        }
        
        BaseResponse<TaskDto> result = await service.AddAsync(dto);

        CreateUpdateEventArgs e = new()
        {
            OperationName = "Add",
            Message = result.Message
        };

        if(result.Value != null)
        {
            e.Dto = result.Value;
        }

        OperationDone?.Invoke(this, e);

        _navigationStore.Previous();
    }        

    private bool CanUpdateTaskExecuteAsync(object? parameter)
    {
        if(string.IsNullOrEmpty(TaskTitle) || string.IsNullOrEmpty(Id))
        {
            return false;
        }
        return true;
    }
    
    public void SetProperties(CreateUpdateEventArgs e)
    {
        Id = e.Dto.Id;
        TaskTitle = e.Dto.Title;
        Priority = e.Dto.Priority;
        Deadline = e.Dto.DeadlineDate ?? DateTime.Now;
    }    

    private async Task UpdateTaskExecuteAsync(object? parameter)
    {
        TaskDto dto = new()
        {
            Id = Id,
            Title = TaskTitle,
            DeadlineDate = (IsLimited) ? Deadline : null,
            Priority = Priority
        };

        using IServiceScope scope = _scopeFactory.CreateScope();

        ITaskService<TaskDto> service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();

        BaseResponse<TaskDto> result = await service.UpdateAsync(dto);

        CreateUpdateEventArgs e = new()
        {
            OperationName = "Update",
            Message = result.Message
        };

        if(result.Value != null)
        {
            e.Dto = result.Value;
        }

        OperationDone?.Invoke(this, e);

        _navigationStore.Previous();
    }
}