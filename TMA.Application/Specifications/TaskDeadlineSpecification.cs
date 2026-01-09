using System.Linq.Expressions;
using TMA.Domain;

namespace TMA.Application.Specifications;

public class TaskDeadlineSpecification(DateTime? date) : ISpecification<TaskEntity>
{
    public DateTime? DeadlineDate { get; } = date;

    public Expression<Func<TaskEntity, bool>> ToExpression()
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TaskEntity), "e");

        ConstantExpression dateConstant = Expression.Constant(DeadlineDate, typeof(DateTime?));

        MemberExpression property = Expression.Property(parameter, nameof(TaskEntity.Deadline));

        BinaryExpression be = Expression.LessThanOrEqual(property, dateConstant);

        return Expression.Lambda<Func<TaskEntity, bool>>(be, parameter);
    }
}