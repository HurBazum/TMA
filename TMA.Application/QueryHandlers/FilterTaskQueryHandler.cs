using System.Linq.Expressions;
using System.Reflection;
using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Application.Others;
using TMA.Application.Queries;
using TMA.Application.Specifications;
using TMA.Application.Specifications.Aggregator;
using TMA.Application.Specifications.Attributes;
using TMA.Domain;

namespace TMA.Application.QueryHandlers;

public class FilterTaskQueryHandler(ITaskRepository repository) : IQueryHandler<FilterTaskQuery, List<TaskDto>>
{
    private readonly ITaskRepository _repository = repository;

    // TaskStatus -> List<TaskStatus> change
    public async Task<List<TaskDto>> HandleAsync(FilterTaskQuery query)
    {
        DomainFilterTaskQuery domainFilter = new()
        {
            Status = query.Status,
            Priority = query.Priority,
            Title = query.Title,
            From = query.From,
            To = query.To
        };

        Func<TaskEntity?, bool> filterExpression = CreateGeneralExpression(domainFilter).Compile();

        IQueryable<TaskEntity?> result = _repository.GetAllAsync();

        IEnumerable<TaskEntity> filtered = result.Where(filterExpression);

        List<TaskDto> list = (filtered.Any()) ? [] : [.. (Enumerable.Select(filtered, i => Transformer.ToDto(i)))];

        return await Task.FromResult(list);
    }


    private static Expression<Func<TaskEntity, bool>> CreateGeneralExpression(DomainFilterTaskQuery query)
    {
        ICollection<ISpecification<TaskEntity>> specifications = [];

        foreach(var pi in query.GetType().GetProperties())
        {
            object? piValue = pi.GetValue(query);

            if(piValue == null)
            {
                continue;
            }

            var attr = pi.GetCustomAttribute<SpecAttribute>();

            if(attr == null)
            {
                continue;
            }

            ISpecification<TaskEntity> specification = Activator.CreateInstance(attr.Specification, piValue)! as ISpecification<TaskEntity>
                ?? throw new Exception($"Cannot create specification of type {attr.Specification.Name}");

            specifications.Add(specification);
        }

        if(specifications.Count == 0)
        {
            ParameterExpression entityParameter = Expression.Parameter(typeof(TaskEntity), "e");
            return Expression.Lambda<Func<TaskEntity, bool>>(Expression.Constant(false), entityParameter);
        }
        
        AndSpecification<TaskEntity> andSpecification = new([.. specifications]);

        return andSpecification.ToExpression();
    }
}