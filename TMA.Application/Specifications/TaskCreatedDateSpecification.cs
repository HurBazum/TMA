using System.Linq.Expressions;
using TMA.Domain;

namespace TMA.Application.Specifications;

public class TaskCreatedDateSpecification(DateTime date) : ISpecification<TaskEntity>
{
    public DateTime CreatedDate { get; } = date;

    public Expression<Func<TaskEntity, bool>> ToExpression()
    {
        ParameterExpression param = Expression.Parameter(typeof(TaskEntity), "t");

        ConstantExpression dateConstant = Expression.Constant(CreatedDate, typeof(DateTime));

        MemberExpression property = Expression.Property(param, nameof(TaskEntity.CreatedDate));

        BinaryExpression be = Expression.GreaterThanOrEqual(property, dateConstant);

        return Expression.Lambda<Func<TaskEntity, bool>>(be, param);
    }
}