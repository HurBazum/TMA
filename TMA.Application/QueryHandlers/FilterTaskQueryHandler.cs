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

        Func<TaskEntity, bool> filterExpression = CreateGeneralExpression(domainFilter).Compile();

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
                "Title" => new TaskTitleSpecification(new(piValue.ToString()!)),
                "To" => new TaskDeadlineSpecification((DateTime)piValue),
                _ => throw new NotImplementedException()
            };

            expressions.Add(specification);
        }

        if(expressions.Count == 0)
        {
            ParameterExpression entityParameter = Expression.Parameter(typeof(TaskEntity), "e");
            return Expression.Lambda<Func<TaskEntity, bool>>(Expression.Constant(true), entityParameter);
        }
        
        AndSpecification<TaskEntity> andSpecification = new([.. expressions]);

        return andSpecification.ToExpression();
    }
}