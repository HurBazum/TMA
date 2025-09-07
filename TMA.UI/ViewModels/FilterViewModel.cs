using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TMA.Infrastructure;
using TMA.UI.Infrastructure.Commands;
using TMA.UI.Infrastructure.Commands.Base;
using TMA.UI.Infrastructure.EventArguments;
using TMA.UI.ViewModels.Base;

namespace TMA.UI.ViewModels;

public class FilterViewModel(ITaskService<TaskDto> taskService) : ViewModelBase
{
    private readonly ITaskService<TaskDto> _taskService = taskService;

    public event EventHandler<FilterDoneEventArgs> SearchIsDone;

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
                OnAddedFilterValue(Title);
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


    #endregion


    #region cmds


    private ICommand? _useAllFiltersCmd;
    public ICommand UseAllFiltersCmd => _useAllFiltersCmd ?? new LambdaCommand(UseAllFiltersCmdExecute, CanUseAllFiltersCmdExecute);

    private bool CanUseAllFiltersCmdExecute(object? parameter) => true;
    private void UseAllFiltersCmdExecute(object? parameter)
    {
        IsAllFilterUse = !IsAllFilterUse;
    }

    private ICommand? _getFilterCmd;
    public ICommand GetFilterFilter => _getFilterCmd ?? new LambdaCommand(GetFilterCmdExecute, CanGetFilterCmdExecuted);

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
        Dictionary<string, string> dic = new();

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
        
    private void OnAddedFilterValue(object propertyValue, [CallerArgumentExpression("propertyValue")]string propertyName = null)
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
        var prefix = filter.Substring(0, underscore);

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
        ParameterExpression dto = System.Linq.Expressions.Expression.Parameter(typeof(object), "d");
        ParameterExpression propName = System.Linq.Expressions.Expression.Parameter(typeof(string), "n");
        ParameterExpression propValue = System.Linq.Expressions.Expression.Parameter(typeof(object), "v");

        MethodCallExpression getDtoTypeCall = System.Linq.Expressions.Expression.Call(dto, typeof(object).GetMethod(nameof(object.GetType))!);
        MethodCallExpression getDtoPropertyCall = System.Linq.Expressions.Expression.Call(getDtoTypeCall, 
            typeof(Type).GetMethod(nameof(Type.GetProperty), new[] { typeof(string) })!, propName);

        MemberExpression propertyType = System.Linq.Expressions.Expression.Property(getDtoPropertyCall, nameof(PropertyInfo.PropertyType));

        MethodCallExpression getUnderlyingTypeCall = System.Linq.Expressions.Expression.Call(typeof(Nullable).GetMethod(nameof(Nullable.GetUnderlyingType), new[] { typeof(Type) })!
            , propertyType);
        
        BinaryExpression checkFoNull = System.Linq.Expressions.Expression.Equal(getUnderlyingTypeCall, System.Linq.Expressions.Expression.Constant(null, typeof(Type)));

        ConditionalExpression getRealType = System.Linq.Expressions.Expression.Condition(checkFoNull, propertyType, getUnderlyingTypeCall);

        MemberExpression isEnum = System.Linq.Expressions.Expression.Property(getRealType, nameof(Type.IsEnum));

        UnaryExpression convertTo = System.Linq.Expressions.Expression.Convert(propValue, typeof(string));

        MethodCallExpression parsedEnum = System.Linq.Expressions.Expression.Call(typeof(Enum).GetMethod(nameof(Enum.Parse), new[] { typeof(Type), typeof(string) })!,
            getRealType, convertTo);

        UnaryExpression convertedEnum = System.Linq.Expressions.Expression.Convert(parsedEnum, typeof(object));

        MethodCallExpression parsedOther = System.Linq.Expressions.Expression.Call(typeof(Convert).GetMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) })!,
            convertTo, propertyType);

        ConditionalExpression condition = System.Linq.Expressions.Expression.Condition(isEnum, convertedEnum, parsedOther);

        MethodCallExpression setValueCall = System.Linq.Expressions.Expression.Call(getDtoPropertyCall, typeof(PropertyInfo).GetMethod(nameof(PropertyInfo.SetValue)
            , new[] { typeof(object), typeof(object) })!, dto, condition);

        return System.Linq.Expressions.Expression.Lambda<Action<object, string, object>>(setValueCall, dto, propName, propValue);
    }
}