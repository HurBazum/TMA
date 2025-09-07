using System.Linq.Expressions;
using TMA.Application.Others;
using TMA.Application.Queries;
using TMA.Domain;

namespace TMA.Application.QueryHandlers;

public class FilterTaskQueryHandler(ITaskRepository repository)
{
    private readonly ITaskRepository _repository = repository;

    public List<TaskEntity> Handle(FilterTaskQuery query)
    {
        DomainFilterTaskQuery domainFilter = new()
        {
            Status = query.Status,
            Priority = query.Priority,
            Title = (!string.IsNullOrEmpty(query.Title)) ? new(query.Title) : null,
            From = query.From,
            To = query.To
        };

        var filterExpression = CreateFilterExpression(domainFilter).Compile();

        IQueryable<TaskEntity?> result = _repository.GetAllAsync();

        IEnumerable<TaskEntity?> filtered = result.Where(filterExpression);

        return [.. filtered];
    }

    private Expression<Func<TaskEntity, bool>> CreateFilterExpression(DomainFilterTaskQuery query)
    {
        ParameterExpression entityParameter = Expression.Parameter(typeof(TaskEntity), "e");

        var expressions = new List<Expression>();

        foreach(var pi in query.GetType().GetProperties())
        {
            var piValue = pi.GetValue(query);

            if(piValue == null)
            {
                continue;
            }

            Type? piUnderlyingType = Nullable.GetUnderlyingType(pi.PropertyType);

            ConstantExpression constant = Expression.Constant(piValue, piUnderlyingType);

            MemberExpression entityProperty = Expression.Property(entityParameter, pi.Name);

            BinaryExpression be = Expression.Equal(entityProperty, constant);

            expressions.Add(be);
        }

        if(expressions.Count == 0)
        {
            return Expression.Lambda<Func<TaskEntity, bool>>(Expression.Constant(true), entityParameter);
        }

        var finalExpression = expressions.Aggregate(Expression.AndAlso);

        return Expression.Lambda<Func<TaskEntity, bool>>(finalExpression, entityParameter);
    }
}