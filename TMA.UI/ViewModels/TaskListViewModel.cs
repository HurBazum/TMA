using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Threading;
using TMA.Application.Dtos;
using TMA.Application.Others;
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
    private readonly INavigationStore _navigationStore;
    private readonly IServiceScopeFactory _scopeFactory;
    public FilterViewModel FilterViewModel { get; }
    public event Action<CreateUpdateEventArgs>? UpdateTask;

    public TaskListViewModel(
        INavigationStore navigationStore,
        FilterViewModel filterViewModel,
        IServiceScopeFactory scopeFactory)
    {
        _navigationStore = navigationStore;
        _scopeFactory = scopeFactory;
        FilterViewModel = filterViewModel;
        FilterViewModel.SearchIsDone += ChangeCollection;
        Dispatcher.CurrentDispatcher.BeginInvoke(() => InitializeCollection());
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
        var nextPage = App.Host.Services.GetRequiredService<CreateUpdateViewModel>();

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

        using IServiceScope scope = _scopeFactory.CreateScope();

        ITaskService<TaskDto> service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();

        BaseResponse<List<TaskDto>> response = await service.FilterTaskAsync(taskDto);

        ShowMessage(response.Message);

        var filteredTVMs = response.Value;

        List<TaskViewModel> newTVMS = [];

        foreach(var filteredTvm in filteredTVMs)
        {
            newTVMS.Add(Infrastructure.Transform.Transformer.ToModel(filteredTvm));
        }

        TVMs = new(newTVMS);
        OnPropertyChanged(nameof(TVMs));
    }



    private void OnTaskOperationDone(object? sender, CreateUpdateEventArgs e)
    {
        ShowMessage(e.Message);
        if(e.OperationName == "Add")
        {
            TVMs.Add(Infrastructure.Transform.Transformer.ToModel(e.Dto));
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

        var nextPage = App.Host.Services.GetRequiredService<CreateUpdateViewModel>();

        nextPage.OperationDone += OnTaskOperationDone;

        UpdateTask += nextPage.SetProperties;

        UpdateTask?.Invoke(new () 
        {            
            Dto = Infrastructure.Transform.Transformer.ToDto(tvm)   
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

        var dto = Infrastructure.Transform.Transformer.ToDto(tvm);

        using IServiceScope scope = _scopeFactory.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();

        var result = await service.UpdateAsync(dto);

        ShowMessage(result.Message);
    }

    #endregion


    #region prvt methods

    private void ChangeCollection(object? sender, FilterDoneEventArgs e)
    {
        var tvms = Infrastructure.Transform.Transformer.ToModel(e.Value);

        TVMs = new(tvms);
        OnPropertyChanged(nameof(TVMs));
        ShowMessage(e.Message);
    }

    private async Task InitializeCollection()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();

        var result = await service.GetAllAsync();

        if(result.Value != null)
        {
            foreach(var item in result.Value)
            { 
                TVMs.Add(Infrastructure.Transform.Transformer.ToModel(item));
            }
        }
    }

    private Expression<Action<object, string, object>> SetFilterProperty()
    {
        ParameterExpression obj = Expression.Parameter(typeof(object), "p");
        ParameterExpression propName = Expression.Parameter(typeof(string), "n");
        ParameterExpression newValue = Expression.Parameter(typeof(object), "v");

        // p.GetType()
        MethodCallExpression getTypeCall =
            Expression.Call(obj, typeof(object).GetMethod(nameof(object.GetType))!);

        // p.GetType().GetProperty(n)
        MethodCallExpression getPropertyCall =
            Expression.Call(getTypeCall,
                typeof(Type).GetMethod(nameof(Type.GetProperty), new[] { typeof(string) })!,
                propName);

        // property.PropertyType
        MemberExpression propertyType =
            Expression.Property(getPropertyCall, nameof(PropertyInfo.PropertyType));

        // Nullable.GetUnderlyingType(property.PropertyType)
        MethodInfo getUnderlyingType = typeof(Nullable).GetMethod(nameof(Nullable.GetUnderlyingType))!;
        MethodCallExpression underlyingTypeCall =
            Expression.Call(getUnderlyingType, propertyType);

        // underlyingType == null ? propertyType : underlyingType
        BinaryExpression underlyingIsNull =
             Expression.Equal(underlyingTypeCall, Expression.Constant(null, typeof(Type)));
        Expression realTargetType =
            Expression.Condition(underlyingIsNull, propertyType, underlyingTypeCall);

        // realTargetType.IsEnum
        MemberExpression isEnum =
            Expression.Property(realTargetType, nameof(Type.IsEnum));

        // (string)v
        UnaryExpression valueToString =
            Expression.Convert(newValue, typeof(string));

        // Enum.Parse(realTargetType, (string)v)
        MethodCallExpression parsedEnum =
            Expression.Call(typeof(Enum).GetMethod(nameof(Enum.Parse), new[] { typeof(Type), typeof(string) })!,
                            realTargetType, valueToString);

        // (PropertyType)Enum.Parse(...)
        Expression convertedEnum =
            Expression.Convert(parsedEnum, typeof(object));

        // Convert.ChangeType(v, propertyType)
        MethodCallExpression convertedOther =
            Expression.Call(typeof(Convert).GetMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) })!,
                            newValue, propertyType);

        // isEnum ? convertedEnum : convertedOther
        ConditionalExpression valueExpression =
            Expression.Condition(isEnum, convertedEnum, convertedOther);

        // property.SetValue(p, valueExpression)
        MethodCallExpression setValueCall =
            Expression.Call(getPropertyCall,
                typeof(PropertyInfo).GetMethod(nameof(PropertyInfo.SetValue),
                                               new[] { typeof(object), typeof(object) })!,
                obj, valueExpression);

        return Expression.Lambda<Action<object, string, object>>(setValueCall, obj, propName, newValue);
    }

    private void ShowMessage(string message)
    {
        Message = message;
        HasMessage = true;
    }

    #endregion
}