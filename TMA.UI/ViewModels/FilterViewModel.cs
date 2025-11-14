using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TMA.Application.Others;
using TMA.Application.Dtos;
using TMA.Application.Services;
using TMA.UI.Infrastructure.Commands;
using TMA.UI.Infrastructure.Commands.Base;
using TMA.UI.Infrastructure.EventArguments;
using TMA.UI.ViewModels.Base;
using System.Collections.ObjectModel;
using TMA.UI.Infrastructure;

namespace TMA.UI.ViewModels;

public class FilterViewModel : ViewModelBase
{
    private readonly ITaskService<TaskDto> _taskService;

    public event EventHandler<FilterDoneEventArgs>? SearchIsDone;

    public FilterViewModel(ITaskService<TaskDto> taskService)
    {
        _taskService = taskService;
    }

    #region properties

    private List<string> _filters = [];

    public List<string> Filters
    {
        get => _filters;
        set => Set(ref _filters, value);
    }

    private bool _isAllFilterUse = false;
    public bool IsAllFilterUse
    {
        get => _isAllFilterUse;
        set => Set(ref _isAllFilterUse, value);
    }

    private string? _title;
    public string? Title
    {
        get => _title;
        set
        {
            Set(ref _title, value);
            if(value != null)
            {
                OnAddedFilterValue(Title!);
            }
        }
    }

    private DateTime? _to;
    public DateTime? To
    {
        get => _to;
        set => Set(ref _to, value);
    }

    private DateTime? _from;
    public DateTime? From
    {
        get => _from;
        set => Set(ref _from, value);
    }


    // ?
    public ObservableCollection<FilterGroup> FilterGroups { get; set; } = 
        [
            new()
            {
                Header = "Priority",
                Options =
                [
                    new("Priority", "None"),
                    new("Priority", "Normal"),
                    new("Priority", "Medium"),
                    new("Priority", "High")
                ]},

            new()
            {
                Header = "Status",
                Options =
                [
                    new("Status", "None"),
                    new("Status", "Pending"),
                    new("Status", "Completed"),
                    new("Status", "Expired")
                ]}
        ];



    #endregion


    #region cmds


    public ICommand UseAllFiltersCmd => new LambdaCommand(UseAllFiltersCmdExecute, CanUseAllFiltersCmdExecute);

    private bool CanUseAllFiltersCmdExecute(object? parameter) => true;
    private void UseAllFiltersCmdExecute(object? parameter)
    {
        IsAllFilterUse = !IsAllFilterUse;
    }

    public ICommand GetFilterFilter => new LambdaCommand(GetFilterCmdExecute, CanGetFilterCmdExecuted);

    private void GetFilterCmdExecute(object? parameter)
    {
        if(parameter is not string value)
        {
            return;
        }

        CheckAndAdd(value);
    }

    private bool CanGetFilterCmdExecuted(object? parameter) => true;

    private IAsyncCommand UseFilterAsyncCmd => new AsyncCommand(ShowFilterCmdExecute, CanShowFilterCmdExecute);
    private CmdAdapter UseFilterAsyncCmdAdapt => new(UseFilterAsyncCmd);

    public ICommand UseFilterCmd => new LambdaCommand(UseFilterAsyncCmdAdapt.Execute, UseFilterAsyncCmdAdapt.CanExecute);

    private bool CanShowFilterCmdExecute(object? parameter) => true;
    private async Task ShowFilterCmdExecute(object? parameter)
    {
        Dictionary<string, string> dic = [];

        FilterDto filterDto = new();

        var expr = FilterInitializer().Compile();

        foreach(string filter in Filters)
        {
            string[] separated = filter.Split('_');

            expr(filterDto, separated[0], separated[1]);            
        }

        var response = await _taskService.FilterTaskAsync(filterDto);

        IsAllFilterUse = false;

        OnFilterDone(response);
    }

    #endregion

    public void OnFilterDone(BaseResponse<List<TaskDto>> response) => SearchIsDone?.Invoke(this, new FilterDoneEventArgs()
    {
        Message = response.Message,
        Value = response.Value
    });
        
    private void OnAddedFilterValue(object propertyValue, [CallerArgumentExpression(nameof(propertyValue))]string propertyName = null)
    {
        if(propertyValue != null)
        {
            string newFilter = $"{propertyName}_{propertyValue}";

            CheckAndAdd(newFilter);
        }
    }

    private void CheckAndAdd(string filter)
    {
        int underscore = filter.IndexOf('_');
        var prefix = filter[..underscore];

        string? item = Filters.FirstOrDefault(x => x.StartsWith(prefix));

        if(!string.IsNullOrEmpty(item))
        {
            Filters.Remove(item);
        }

        if(underscore != filter.Length - 1 && !filter.Contains("_None"))
        {
            Filters.Add(filter);
        }
    }

    // перенести фильтрацию сюда, 
    // добавить фильтрацию по дате: меньше конкретной даты/больше конкретной даты   
    // также добавить там подписку на событие FilterIsDone и метод для оброботки результатов этой прослушки

    private Expression<Action<object, string, object>> FilterInitializer()
    {
        ParameterExpression dto = Expression.Parameter(typeof(object), "d");
        ParameterExpression propName = Expression.Parameter(typeof(string), "n");
        ParameterExpression propValue = Expression.Parameter(typeof(object), "v");

        MethodCallExpression getDtoTypeCall = Expression.Call(dto, typeof(object).GetMethod(nameof(object.GetType))!);
        MethodCallExpression getDtoPropertyCall = Expression.Call(getDtoTypeCall, 
            typeof(Type).GetMethod(nameof(Type.GetProperty), [typeof(string)])!, propName);

        MemberExpression propertyType = Expression.Property(getDtoPropertyCall, nameof(PropertyInfo.PropertyType));

        MethodCallExpression getUnderlyingTypeCall = Expression.Call(typeof(Nullable).GetMethod(nameof(Nullable.GetUnderlyingType), [typeof(Type)])!
            , propertyType);
        
        BinaryExpression checkFoNull = Expression.Equal(getUnderlyingTypeCall, Expression.Constant(null, typeof(Type)));

        ConditionalExpression getRealType = Expression.Condition(checkFoNull, propertyType, getUnderlyingTypeCall);

        MemberExpression isEnum = Expression.Property(getRealType, nameof(Type.IsEnum));

        UnaryExpression convertTo = Expression.Convert(propValue, typeof(string));

        MethodCallExpression parsedEnum = Expression.Call(typeof(Enum).GetMethod(nameof(Enum.Parse), [typeof(Type), typeof(string)])!,
            getRealType, convertTo);

        UnaryExpression convertedEnum = Expression.Convert(parsedEnum, typeof(object));

        MethodCallExpression parsedOther = Expression.Call(typeof(Convert).GetMethod(nameof(Convert.ChangeType), [typeof(object), typeof(Type)])!,
            convertTo, propertyType);

        ConditionalExpression condition = Expression.Condition(isEnum, convertedEnum, parsedOther);

        MethodCallExpression setValueCall = Expression.Call(getDtoPropertyCall, typeof(PropertyInfo).GetMethod(nameof(PropertyInfo.SetValue)
            , [typeof(object), typeof(object)])!, dto, condition);

        return Expression.Lambda<Action<object, string, object>>(setValueCall, dto, propName, propValue);
    }
}