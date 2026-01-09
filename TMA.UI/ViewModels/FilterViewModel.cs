using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TMA.Application.Dtos;
using TMA.Application.Dtos.DtoAttributes;
using TMA.Application.Others;
using TMA.Application.Services;
using TMA.UI.Infrastructure;
using TMA.UI.Infrastructure.Commands;
using TMA.UI.Infrastructure.Commands.Base;
using TMA.UI.Infrastructure.Commands.SpecialCommands;
using TMA.UI.Infrastructure.EventArguments;
using TMA.UI.ViewModels.Base;

namespace TMA.UI.ViewModels;

public class FilterViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _factory;

    public event EventHandler<FilterDoneEventArgs>? SearchIsDone;

    public LambdaCommand UniversalDateCmd { get; private set; }
    public Dictionary<string, PropertyAccessor<DateTime?>> _dates;

    public FilterViewModel(IServiceScopeFactory factory)
    {
        _factory = factory;

        _dates = new()
        {
            {
                "From",
                new PropertyAccessor<DateTime?>()
                {
                    Getter = () => From,
                    Setter = (x) => From = x
                }
            },
            {
                "To",
                new PropertyAccessor<DateTime?>()
                {
                    Getter = () => To,
                    Setter = (x) => To = x
                }
            }
        };

        UniversalDateCmd = CommandCreator.CreateDateCmd(_dates);
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

    private DateTime? _to = DateTime.Today;
    public DateTime? To
    {
        get => _to;
        set
        {
            Set(ref _to, value);
            OnAddedFilterValue(To!);
        }
    }

    private DateTime? _from = DateTime.Today;
    public DateTime? From
    {
        get => _from;
        set
        {
            Set(ref _from, value);
            OnAddedFilterValue(From!);
        }
    }

    private bool _useTo = false;
    public bool UseTo
    {
        get => _useTo;
        set => Set(ref _useTo, value);
    }

    private bool _useFrom = false;
    public bool UseFrom
    {
        get => _useFrom;
        set => Set(ref _useFrom, value);
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

        using IServiceScope scope = _factory.CreateScope();
        ITaskService<TaskDto> service = scope.ServiceProvider.GetRequiredService<ITaskService<TaskDto>>();
        BaseResponse<List<TaskDto>> response = await service.FilterTaskAsync(filterDto);
        
        IsAllFilterUse = false;

        OnFilterDone(response);
    }

    #endregion

    public void OnFilterDone(BaseResponse<List<TaskDto>> response) => SearchIsDone?.Invoke(this, new FilterDoneEventArgs()
    {
        Message = response.Message,
        Value = response.Value
    });
        
    private void OnAddedFilterValue(object propertyValue, [CallerArgumentExpression(nameof(propertyValue))]string? propertyName = null)
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
            Expression.Call(typeof(Convert).GetMethod(nameof(Convert.ChangeType), [typeof(object), typeof(Type), typeof(IFormatProvider)])!, 
            newValue, realTargetType, Expression.Constant(System.Globalization.CultureInfo.CurrentCulture));

        // isEnum ? convertedEnum : convertedOther
        ConditionalExpression valueExpression = Expression.Condition(isEnum, convertedEnum, convertedOther);

        // property.SetValue(p, valueExpression)
        MethodCallExpression setValueCall = Expression
            .Call(getPropertyCall, typeof(PropertyInfo).GetMethod(nameof(PropertyInfo.SetValue), [typeof(object), typeof(object)])!, obj, valueExpression);

        return Expression.Lambda<Action<object, string, object>>(setValueCall, obj, propName, newValue);
    }
}