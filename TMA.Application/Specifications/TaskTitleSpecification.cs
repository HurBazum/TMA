using System.Linq.Expressions;
using TMA.Domain;
using TMA.Domain.VOs;

namespace TMA.Application.Specifications;

internal class TaskTitleSpecification(TaskTitle title) : ISpecification<TaskEntity>
{
    public TaskTitle Title { get; } = title;

    public Expression<Func<TaskEntity, bool>> ToExpression()
    {
        ParameterExpression param = Expression.Parameter(typeof(TaskEntity), "e");
        ConstantExpression title = Expression.Constant(Title, typeof(TaskTitle));

        MemberExpression prop = Expression.Property(param, nameof(TaskEntity.Title));
        MemberExpression propValue = Expression.Property(prop, nameof(TaskTitle.Value));
        MemberExpression titleValue = Expression.Property(title, nameof(TaskTitle.Value));
        MethodCallExpression call = Expression.Call(propValue, typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!, titleValue);

        return Expression.Lambda<Func<TaskEntity, bool>>(call, param);
    }
}