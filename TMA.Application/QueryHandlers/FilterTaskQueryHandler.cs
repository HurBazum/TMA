using System.Linq.Expressions;
using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Application.Others;
using TMA.Application.Queries;
using TMA.Application.Specifications;
using TMA.Application.Specifications.Aggregator;
using TMA.Domain;

namespace TMA.Application.QueryHandlers;

public class FilterTaskQueryHandler(ITaskRepository repository) : IQueryHandler<FilterTaskQuery, List<TaskDto>>
{
    private readonly ITaskRepository _repository = repository;

    public async Task<List<TaskDto>> HandleAsync(FilterTaskQuery query)
    {
        DomainFilterTaskQuery domainFilter = new()
        {
            Status = query.Status,
            Priority = query.Priority,
            Title = (!string.IsNullOrEmpty(query.Title)) ? new(query.Title) : null,
            From = query.From,
            To = query.To
        };

        var filterExpression = CreateGeneralExpression(domainFilter).Compile();

        IQueryable<TaskEntity?> result = _repository.GetAllAsync();

        IEnumerable<TaskEntity?> filtered = result.Where(filterExpression);

        List<TaskDto> list = [.. (Enumerable.Select(filtered, i => Transformer.ToDto(i)))];

        return await Task.FromResult(list);
    }

    private Expression<Func<TaskEntity, bool>> CreateGeneralExpression(DomainFilterTaskQuery query)
    {
        ICollection<ISpecification<TaskEntity>> expressions = [];

        foreach(var pi in query.GetType().GetProperties())
        {
            object? piValue = pi.GetValue(query);

            if(piValue == null)
            {
                continue;
            }

            ISpecification<TaskEntity> specification = pi.Name switch
            {
                "Status" => new TaskStatusSpecification((Shared.TaskStatus)piValue),
                "Priority" => new TaskPrioritySpecification((Shared.TaskPriority)piValue),
                "Title" => new TaskTitleSpecification(new(piValue.ToString())),
                _ => throw new NotImplementedException()
            };

            expressions.Add(specification);
        }

        if(expressions.Count == 0)
        {
            ParameterExpression entityParameter = Expression.Parameter(typeof(TaskEntity), "e");
            return Expression.Lambda<Func<TaskEntity, bool>>(Expression.Constant(true), entityParameter);
        }
        
        AndSpecification<TaskEntity> andSpecification = new(expressions.ToArray());

        return andSpecification.ToExpression();
    }

    // ISpecification
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