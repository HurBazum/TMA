using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Threading;
using TMA.Application.Dtos;
using TMA.Application.Others;
using TMA.Application.Others.Args;
using TMA.Application.Services;
using TMA.UI.Infrastructure.Commands;
using TMA.UI.Infrastructure.Commands.Base;
using TMA.UI.Infrastructure.EventArguments;
using TMA.UI.Infrastructure.Stores;
using TMA.UI.Models;
using TMA.UI.ViewModels.Base;

namespace TMA.UI.ViewModels;

public class TaskListViewModel : ViewModelBase
{
    private readonly CancellationTokenSource _cts = new();

    private readonly INavigationStore _navigationStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CheckTaskService _checkTaskService;
    public FilterViewModel FilterViewModel { get; }
    public event Action<CreateUpdateEventArgs>? UpdateTask;

    public ISnackbarMessageQueue MessageQueue { get; set; } = new SnackbarMessageQueue();

    public TaskListViewModel(
        INavigationStore navigationStore,
        FilterViewModel filterViewModel,
        IServiceScopeFactory scopeFactory,
        CheckTaskService checkTaskService)
    {
        _navigationStore = navigationStore;
        _scopeFactory = scopeFactory;

        FilterViewModel = filterViewModel;
        FilterViewModel.SearchIsDone += ChangeCollection;
        Dispatcher.CurrentDispatcher.BeginInvoke(() => InitializeCollection());

        _checkTaskService = checkTaskService;
        _checkTaskService.ExpiredTask += DeleteExpiredTVMs;
        Dispatcher.CurrentDispatcher.InvokeAsync(() => _checkTaskService.CheckTasksDeadlineAsync(_cts.Token));        
    }

    #region properties

    private string? _message;
    public string? Message
    {
        get => _message;
        set => Set(ref _message, value);
    }

    private bool _hasMessage;
    public bool HasMessage
    {
        get => _hasMessage;
        set => Set(ref _hasMessage, value);
    }

    private TaskViewModel? _selectedTask;
    public TaskViewModel? SelectedTask
    {
        get => _selectedTask;
        set => Set(ref _selectedTask, value);
    }

    private List<string> _filters = [];

    public List<string> Filters
    {
        get => _filters;
        set => Set(ref _filters, value);
    }

    public ObservableCollection<TaskViewModel> TVMs { get; set; } = [];

    #endregion

    #region cmds


    private IAsyncCommand FilterTaskAsyncCmd => new AsyncCommand(FilterTVMCmdExecute, CanFilterTVMCmdExecute);

    public ICommand CreateTVMCmd => new LambdaCommand(CreateTVMCmdExecuted, CanCreateTVMCmdExecute);

    private CmdAdapter FilterTMVAdapt => new(FilterTaskAsyncCmd);

    public LambdaCommand FilterTVMCmd => new(FilterTMVAdapt.Execute, FilterTMVAdapt.CanExecute);

    public ICommand SetFilterValueCmd => new LambdaCommand(SetFilterValueCmdExecute, parameter => true);

    private void SetFilterValueCmdExecute(object? parameter)
    {
        if(parameter is not string value)
        {
            return;
        }

        int zPosition = value.IndexOf('_');
        string t = value[..zPosition];

        var search = Filters.FirstOrDefault(x => x.StartsWith(t));

        if(search != null)
        {
            Filters.Remove(search);
        }

        Filters.Add(value);
    }

    private bool CanCreateTVMCmdExecute(object? parameter) => true;

    private void CreateTVMCmdExecuted(object? parameter)
    {
        var nextPage = App.Host.Services.GetRequiredService<CreateUpdateViewModel>();

        nextPage.OperationDone += OnTaskOperationDone;

        _navigationStore.Next(nextPage);
    }

    private bool CanFilterTVMCmdExecute(object? parameter) => true;
    private async Task FilterTVMCmdExecute(object? parameter)
    {
        Dictionary<string, string> dic = [];

        foreach(string filter in Filters)
        {
            var divided = filter.Split('_');
            dic.Add(divided[0], divided[1]);
        }


        FilterDto taskDto = new();

        var setter = SetFilterProperty().Compile();

        foreach(var kvp in dic)
        {
            if(kvp.Value != "None")
            {
                setter(taskDto, kvp.Key, kvp.Value);
            }
        }

        using IServiceScope scope = _scopeFactory.CreateScope();

        ITaskService<TaskDto> service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();

        BaseResponse<List<TaskDto>> response = await service.FilterTaskAsync(taskDto);

        ShowMessage(response.Message);

        var filteredTVMs = response.Value;

        List<TaskViewModel> newTVMS = [];

        foreach(var filteredTvm in filteredTVMs!)
        {
            newTVMS.Add(Infrastructure.Transform.Transformer.ToModel(filteredTvm));
        }

        TVMs = new(newTVMS);
        OnPropertyChanged(nameof(TVMs));
    }



    private void OnTaskOperationDone(object? sender, CreateUpdateEventArgs e)
    {
        ShowMessage(e.Message!);
        if(e.OperationName == "Add")
        {
            TVMs.Add(Infrastructure.Transform.Transformer.ToModel(e.Dto!));
        }
        if(e.OperationName == "Update")
        {
            var tvm = TVMs.First(x => x.Id == e.Dto!.Id);
            if(e.Dto!.DeadlineDate is not null)
            {
                tvm.Deadline = e.Dto.DeadlineDate;
            }
            tvm.Title = e.Dto.Title;
            tvm.Priority = e.Dto.Priority;            
        }
    }

    public ICommand UpdateTaskCmd => new LambdaCommand(UpdateTaskExecuted, CanUpdateTaskCmdExecute);

    private bool CanUpdateTaskCmdExecute(object? parameter)
    {
        if(parameter is not TaskViewModel tvm 
            || tvm.Status != Shared.TaskStatus.Pending)
        {
            return false;
        }

        return true;
    }

    private void UpdateTaskExecuted(object? parameter)
    {
        var tvm = parameter as TaskViewModel;

        var nextPage = App.Host.Services.GetRequiredService<CreateUpdateViewModel>();

        nextPage.OperationDone += OnTaskOperationDone;

        UpdateTask += nextPage.SetProperties;

        UpdateTask?.Invoke(new () 
        {            
            Dto = Infrastructure.Transform.Transformer.ToDto(tvm!)   
        });

        _navigationStore.Next(nextPage);
    }

    //private readonly ICommand? _completeTaskCmd;
    public ICommand CompleteTaskCmd => new LambdaCommand(CompleteTaskCmdExecute, CanCompleteTaskCmdExecute);

    private bool CanCompleteTaskCmdExecute(object? parameter)
    {
        if(parameter is not TaskViewModel tvm || tvm.Status != Shared.TaskStatus.Pending)
        {
            return false;
        }
        return true;
    }

    private async void CompleteTaskCmdExecute(object? parameter)
    {
        TaskViewModel tvm = parameter as TaskViewModel;

        tvm.Status = Shared.TaskStatus.Completed;

        TaskDto dto = Infrastructure.Transform.Transformer.ToDto(tvm);

        using IServiceScope scope = _scopeFactory.CreateScope();

        ITaskService<TaskDto> service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();

        BaseResponse<TaskDto> result = await service.UpdateAsync(dto);

        ShowMessage(result.Message);
    }

    #endregion


    #region prvt methods

    public void DeleteExpiredTVMs(object? sender, ExpiredTaskEventArgs e)
    {
        TaskViewModel tvm = TVMs.Single(x => x.Id == e.DtoId);

        TVMs.Remove(tvm);
        OnPropertyChanged(nameof(TVMs));

        ShowMessage(e.Message);
    }

    private void ChangeCollection(object? sender, FilterDoneEventArgs e)
    {
        var tvms = Infrastructure.Transform.Transformer.ToModel(e.Value!);

        TVMs = new(tvms);
        OnPropertyChanged(nameof(TVMs));
        ShowMessage(e.Message!);
    }

    private async Task InitializeCollection()
    {
        FilterDto filter = new()
        {
            Status = Shared.TaskStatus.Pending
        };

        using IServiceScope scope = _scopeFactory.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();

        var result = await service.FilterTaskAsync(filter);

        if(result.Value != null)
        {
            foreach(var item in result.Value)
            { 
                TVMs.Add(Infrastructure.Transform.Transformer.ToModel(item));
            }

            ShowMessage("Задачи успешно получены");
        }
    }

    private Expression<Action<object, string, object>> SetFilterProperty()
    {
        ParameterExpression obj = Expression.Parameter(typeof(object), "p");
        ParameterExpression propName = Expression.Parameter(typeof(string), "n");
        ParameterExpression newValue = Expression.Parameter(typeof(object), "v");

        // p.GetType()
        MethodCallExpression getTypeCall = Expression.Call(obj, typeof(object).GetMethod(nameof(object.GetType))!);

        // p.GetType().GetProperty(n)
        MethodCallExpression getPropertyCall = Expression.Call(getTypeCall, typeof(Type).GetMethod(nameof(Type.GetProperty), [typeof(string)])!, propName);

        // property.PropertyType
        MemberExpression propertyType = Expression.Property(getPropertyCall, nameof(PropertyInfo.PropertyType));

        // Nullable.GetUnderlyingType(property.PropertyType)
        MethodInfo getUnderlyingType = typeof(Nullable).GetMethod(nameof(Nullable.GetUnderlyingType))!;
        MethodCallExpression underlyingTypeCall = Expression.Call(getUnderlyingType, propertyType);

        // underlyingType == null ? propertyType : underlyingType
        BinaryExpression underlyingIsNull = Expression.Equal(underlyingTypeCall, Expression.Constant(null, typeof(Type)));
        Expression realTargetType = Expression.Condition(underlyingIsNull, propertyType, underlyingTypeCall);

        // realTargetType.IsEnum
        MemberExpression isEnum = Expression.Property(realTargetType, nameof(Type.IsEnum));

        // (string)v
        UnaryExpression valueToString = Expression.Convert(newValue, typeof(string));

        // Enum.Parse(realTargetType, (string)v)
        MethodCallExpression parsedEnum =
            Expression.Call(typeof(Enum).GetMethod(nameof(Enum.Parse), [typeof(Type), typeof(string)])!, realTargetType, valueToString);

        // (PropertyType)Enum.Parse(...)
        Expression convertedEnum = Expression.Convert(parsedEnum, typeof(object));

        // Convert.ChangeType(v, propertyType)
        MethodCallExpression convertedOther =
            Expression.Call(typeof(Convert).GetMethod(nameof(Convert.ChangeType), [typeof(object), typeof(Type)])!, newValue, propertyType);

        // isEnum ? convertedEnum : convertedOther
        ConditionalExpression valueExpression = Expression.Condition(isEnum, convertedEnum, convertedOther);

        // property.SetValue(p, valueExpression)
        MethodCallExpression setValueCall = Expression
            .Call(getPropertyCall, typeof(PropertyInfo).GetMethod(nameof(PropertyInfo.SetValue), [typeof(object), typeof(object)])!, obj, valueExpression);

        return Expression.Lambda<Action<object, string, object>>(setValueCall, obj, propName, newValue);
    }

    private void ShowMessage(string message) => App.Current.Dispatcher.InvokeAsync(() => MessageQueue.Enqueue(message));

    #endregion
}