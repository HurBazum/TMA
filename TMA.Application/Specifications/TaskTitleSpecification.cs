using System.Linq.Expressions;
using TMA.Domain;
using TMA.Domain.VOs;

namespace TMA.Application.Specifications;

internal class TaskTitleSpecification(string title) : ISpecification<TaskEntity>
{
    public string Title { get; } = title;

    public Expression<Func<TaskEntity, bool>> ToExpression()
    {
        ParameterExpression param = Expression.Parameter(typeof(TaskEntity), "e");
        ConstantExpression title = Expression.Constant(Title, typeof(string));

        MemberExpression prop = Expression.Property(param, nameof(TaskEntity.Title));
        MemberExpression propValue = Expression.Property(prop, nameof(TaskTitle.Value));

        MethodCallExpression call = Expression.Call(propValue, typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!, title);

        return Expression.Lambda<Func<TaskEntity, bool>>(call, param);
    }
}