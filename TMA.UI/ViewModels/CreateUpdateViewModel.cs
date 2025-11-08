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

    private IAsyncCommand _addTaskCmd => new AsyncCommand(AddTaskCmdExecute, CanAddTaskCmdExecuted);
    private IAsyncCommand _updateTaskAsyncCmd => new AsyncCommand(UpdateTaskExecuteAsync, CanUpdateTaskExecuteAsync);
    
    private ICommand? _changeValueCmd;

    private enum DataValue
    {
        Day,
        Month,
        Year,
        Priority
    }

    public ICommand ChangeValueCmd => _changeValueCmd ?? new LambdaCommand(ChangeValueCmdExecute, CanChangeValueCmdExecute);    
    public CmdAdapter AddTaskCmdAdapt => new(_addTaskCmd);
    private CmdAdapter UpdateTaskAsyncCmdAdapt => new(_updateTaskAsyncCmd);

    
    public LambdaCommand AddTaskCmd => new(AddTaskCmdAdapt.Execute, AddTaskCmdAdapt.CanExecute);
    public LambdaCommand UpdateTaskCmd => new(UpdateTaskAsyncCmdAdapt.Execute, UpdateTaskAsyncCmdAdapt.CanExecute);
    public LambdaCommand GoBackCmd => new((object p) => { _navigationStore.Previous(); }, (object p) => true);

    private bool CanChangeValueCmdExecute(object parameter) => true;
    private void ChangeValueCmdExecute(object parameter)
    {
        string obj = parameter as string;

        if(string.IsNullOrEmpty(obj))
        {
            return;
        }

        string[] propAction = obj.Split('_');

        bool isIncrement = (propAction[1] == "Increment");

        DataValue data = Enum.Parse<DataValue>(propAction[0]);

        switch(data)
        {
            case DataValue.Day:
                if(isIncrement)
                {
                    Deadline = Deadline.Value.AddDays(1);
                }
                else
                {
                    Deadline = Deadline.Value.AddDays(-1);
                }
                ;
                break;
            case DataValue.Month:
                if(isIncrement)
                {
                    Deadline = Deadline.Value.AddMonths(1);
                }
                else
                {
                    Deadline = Deadline.Value.AddMonths(-1);
                }
                ;
                break;
            case DataValue.Year:
                if(isIncrement)
                {
                    Deadline = Deadline.Value.AddYears(1);
                }
                else
                {
                    Deadline = Deadline.Value.AddYears(-1);
                }
                ;
                break;
            case DataValue.Priority:
                if(isIncrement)
                {
                    Priority = ((int)Priority < 2) ? Priority + 1 : TaskPriority.Normal;
                }
                else
                {
                    Priority = ((int)Priority > 0) ? Priority - 1 : TaskPriority.High;
                }
                ;
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

        var result = await service.AddAsync(dto);
        
        if(result.Value != null)
        {
            OperationDone?.Invoke(this, new CreateUpdateEventArgs() 
            {
                OperationName = "Add",
                Dto = result.Value,
                Message = result.Message
            });
        }

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

        var result = await service.UpdateAsync(dto);

        if(result.Value != null)
        {
            OperationDone?.Invoke(this, new CreateUpdateEventArgs() 
            { 
                OperationName = "Update", 
                Dto = dto,
                Message = result.Message
            });
        }

        _navigationStore.Previous();
    }
}