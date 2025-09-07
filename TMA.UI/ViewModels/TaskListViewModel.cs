using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Threading;
using TMA.Infrastructure;
using TMA.UI.Infrastructure.Commands;
using TMA.UI.Infrastructure.Commands.Base;
using TMA.UI.Infrastructure.EventArguments;
using TMA.UI.Infrastructure.Stores;
using TMA.UI.Infrastructure.Transform;
using TMA.UI.Models;
using TMA.UI.ViewModels.Base;

namespace TMA.UI.ViewModels;

public class TaskListViewModel : ViewModelBase
{
    private readonly INavigationStore _navigationStore;
    private readonly ITaskService<TaskDto> _taskService;
    public FilterViewModel FilterViewModel { get; }
    public event EventHandler<CreateUpdateEventArgs>? UpdateTask;

    public TaskListViewModel(INavigationStore navigationStore, ITaskService<TaskDto> taskService)
    {
        _navigationStore = navigationStore;
        _taskService = taskService;
        FilterViewModel = App.Provider.GetRequiredService<FilterViewModel>();
        FilterViewModel.SearchIsDone += ChangeCollection;
        Dispatcher.CurrentDispatcher.BeginInvoke(() => InitializeCollection());
    }

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


    #region cmds


    private IAsyncCommand filterTaskAsyncCmd => new AsyncCommand(FilterTVMCmdExecute, CanFilterTVMCmdExecute);

    private ICommand? _createTVMCmd;
    public ICommand CreateTVMCmd => _createTVMCmd ?? new LambdaCommand(CreateTVMCmdExecuted, CanCreateTVMCmdExecute);

    private CmdAdapter FilterTMVAdapt => new(filterTaskAsyncCmd);

    public LambdaCommand FilterTVMCmd => new(FilterTMVAdapt.Execute, FilterTMVAdapt.CanExecute);

    private ICommand? _setFilterValueCmd;
    public ICommand SetFilterValueCmd => _setFilterValueCmd ?? new LambdaCommand(SetFilterValueCmdExecute, parameter => true);

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
        var nextPage = App.Provider.GetRequiredService<CreateUpdateViewModel>();

        nextPage.OperationDone += OnTaskOperationDone;

        _navigationStore.Next(nextPage);
    }

    private bool CanFilterTVMCmdExecute(object? parameter) => true;
    private async Task FilterTVMCmdExecute(object? parameter)
    {
        Dictionary<string, string> dic = new();

        foreach(string filter in Filters)
        {
            var divided = filter.Split('_');
            dic.Add(divided[0], divided[1]);
        }


        FilterDto taskDto = new();

        var setter = SetFilterProperty().Compile();

        bool isFiltered = false;
        foreach(var kvp in dic)
        {
            if(kvp.Value != "None")
            {
                isFiltered = true;
                setter(taskDto, kvp.Key, kvp.Value);
            }
        }

        var response = await _taskService.FilterTaskAsync(taskDto);

        Message = response.Message;

        HasMessage = true;

        var filteredTVMs = response.Value;

        List<TaskViewModel> newTVMS = [];

        foreach(var filteredTvm in filteredTVMs)
        {
            newTVMS.Add(Transformer.ToModel(filteredTvm));
        }

        TVMs = new(newTVMS);
        OnPropertyChanged(nameof(TVMs));
    }



    private void OnTaskOperationDone(object? sender, CreateUpdateEventArgs e)
    {
        Message = e.Message;
        HasMessage = true;
        if(e.OperationName == "Add")
        {
            TVMs.Add(Transformer.ToModel(e.Dto));
        }
        if(e.OperationName == "Update")
        {
            var tvm = TVMs.First(x => x.Id == e.Dto.Id);
            if(e.Dto.DeadlineDate is not null)
            {
                tvm.Deadline = e.Dto.DeadlineDate;
            }
            tvm.Title = e.Dto.Title;
            tvm.Priority = e.Dto.Priority;            
        }
    }

    private ICommand? _updateTaskCmd;
    public ICommand UpdateTaskCmd => _updateTaskCmd ?? new LambdaCommand(UpdateTaskExecuted, CanUpdateTaskCmdExecute);

    private bool CanUpdateTaskCmdExecute(object? parameter)
    {
        var tvm = parameter as TaskViewModel;

        if(tvm is null || tvm.Completed)
        {
            return false;
        }

        return true;
    }

    private void UpdateTaskExecuted(object? parameter)
    {
        var tvm = parameter as TaskViewModel;

        var nextPage = App.Provider.GetRequiredService<CreateUpdateViewModel>();

        nextPage.OperationDone += OnTaskOperationDone;

        UpdateTask += nextPage.SetProperties;

        UpdateTask?.Invoke(this, new () 
        {            
            Dto = Transformer.ToDto(tvm)   
        });

        _navigationStore.Next(nextPage);
    }

    private ICommand? _completeTaskCmd;
    public ICommand CompleteTaskCmd => _completeTaskCmd ?? new LambdaCommand(CompleteTaskCmdExecute, CanCompleteTaskCmdExecute);

    private bool CanCompleteTaskCmdExecute(object? parameter)
    {
        var tvm = parameter as TaskViewModel;

        if(tvm.Completed == true)
        {
            return false;
        }
        return true;
    }

    private async void CompleteTaskCmdExecute(object? parameter)
    {
        var tvm = parameter as TaskViewModel;

        tvm.Completed = true;

        var dto = Transformer.ToDto(tvm);

        var result = await _taskService.UpdateAsync(dto);

        Message = result.Message;
        HasMessage = true;
    }

    #endregion

    private void ChangeCollection(object? sender, FilterDoneEventArgs e)
    {
        var tvms = Transformer.ToModel(e.Value);

        TVMs = new(tvms);
        OnPropertyChanged(nameof(TVMs));
        Message = e.Message;
        HasMessage = true;
    }

    private async Task InitializeCollection()
    {
        var result = await _taskService.GetAllAsync();

        if(result.Value != null)
        {
            foreach(var item in result.Value)
            { 
                TVMs.Add(Transformer.ToModel(item));
            }
        }
    }

    private Expression<Action<object, string, object>> SetFilterProperty()
    {
        ParameterExpression obj = System.Linq.Expressions.Expression.Parameter(typeof(object), "p");
        ParameterExpression propName = System.Linq.Expressions.Expression.Parameter(typeof(string), "n");
        ParameterExpression newValue = System.Linq.Expressions.Expression.Parameter(typeof(object), "v");

        // p.GetType()
        MethodCallExpression getTypeCall =
            System.Linq.Expressions.Expression.Call(obj, typeof(object).GetMethod(nameof(object.GetType))!);

        // p.GetType().GetProperty(n)
        MethodCallExpression getPropertyCall =
            System.Linq.Expressions.Expression.Call(getTypeCall,
                typeof(Type).GetMethod(nameof(Type.GetProperty), new[] { typeof(string) })!,
                propName);

        // property.PropertyType
        MemberExpression propertyType =
            System.Linq.Expressions.Expression.Property(getPropertyCall, nameof(PropertyInfo.PropertyType));

        // Nullable.GetUnderlyingType(property.PropertyType)
        MethodInfo getUnderlyingType = typeof(Nullable).GetMethod(nameof(Nullable.GetUnderlyingType))!;
        MethodCallExpression underlyingTypeCall =
            System.Linq.Expressions.Expression.Call(getUnderlyingType, propertyType);

        // underlyingType == null ? propertyType : underlyingType
        BinaryExpression underlyingIsNull =
             System.Linq.Expressions.Expression.Equal(underlyingTypeCall, System.Linq.Expressions.Expression.Constant(null, typeof(Type)));
        System.Linq.Expressions.Expression realTargetType =
            System.Linq.Expressions.Expression.Condition(underlyingIsNull, propertyType, underlyingTypeCall);

        // realTargetType.IsEnum
        MemberExpression isEnum =
            System.Linq.Expressions.Expression.Property(realTargetType, nameof(Type.IsEnum));

        // (string)v
        UnaryExpression valueToString =
            System.Linq.Expressions.Expression.Convert(newValue, typeof(string));

        // Enum.Parse(realTargetType, (string)v)
        MethodCallExpression parsedEnum =
            System.Linq.Expressions.Expression.Call(typeof(Enum).GetMethod(nameof(Enum.Parse), new[] { typeof(Type), typeof(string) })!,
                            realTargetType, valueToString);

        // (PropertyType)Enum.Parse(...)
        System.Linq.Expressions.Expression convertedEnum =
            System.Linq.Expressions.Expression.Convert(parsedEnum, typeof(object));

        // Convert.ChangeType(v, propertyType)
        MethodCallExpression convertedOther =
            System.Linq.Expressions.Expression.Call(typeof(Convert).GetMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) })!,
                            newValue, propertyType);

        // isEnum ? convertedEnum : convertedOther
        ConditionalExpression valueExpression =
            System.Linq.Expressions.Expression.Condition(isEnum, convertedEnum, convertedOther);

        // property.SetValue(p, valueExpression)
        MethodCallExpression setValueCall =
            System.Linq.Expressions.Expression.Call(getPropertyCall,
                typeof(PropertyInfo).GetMethod(nameof(PropertyInfo.SetValue),
                                               new[] { typeof(object), typeof(object) })!,
                obj, valueExpression);

        return System.Linq.Expressions.Expression.Lambda<Action<object, string, object>>(setValueCall, obj, propName, newValue);
    }
}