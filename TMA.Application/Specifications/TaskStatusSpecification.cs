using System.Linq.Expressions;
using TMA.Domain;

namespace TMA.Application.Specifications
{
    internal class TaskStatusSpecification(Shared.TaskStatus status) : ISpecification<TaskEntity>
    {
        public Shared.TaskStatus Status { get; } = status;

        public Expression<Func<TaskEntity, bool>> ToExpression()
        {
            ParameterExpression param = Expression.Parameter(typeof(TaskEntity), "e");
            ConstantExpression status = Expression.Constant(Status, typeof(Shared.TaskStatus));

            MemberExpression prop = Expression.Property(param, nameof(TaskEntity.Status));
            BinaryExpression be = Expression.Equal(prop, status);

            return Expression.Lambda<Func<TaskEntity, bool>>(be, param);
        }
    }
}
