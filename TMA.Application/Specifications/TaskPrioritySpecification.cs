using System.Linq.Expressions;
using TMA.Domain;
using TMA.Shared;

namespace TMA.Application.Specifications;

internal class TaskPrioritySpecification(TaskPriority priority) : ISpecification<TaskEntity>
{
    public TaskPriority Priority { get; } = priority;

    public Expression<Func<TaskEntity, bool>> ToExpression()
    {
        ParameterExpression param = Expression.Parameter(typeof(TaskEntity), "e");
        ConstantExpression priority = Expression.Constant(Priority, typeof(TaskPriority));

        MemberExpression prop = Expression.Property(param, nameof(TaskEntity.Priority));
        BinaryExpression be = Expression.Equal(prop, priority);

        return Expression.Lambda<Func<TaskEntity, bool>>(be, param);
    }
}