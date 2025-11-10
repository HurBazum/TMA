using System.Linq.Expressions;

namespace TMA.Application.Specifications.Aggregator;

internal class AndSpecification<T>(params ISpecification<T>[] specifications) : ISpecification<T>
{
    public IEnumerable<ISpecification<T>> Specifications { get; } = specifications;

    public Expression<Func<T, bool>> ToExpression()
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "t");

        IEnumerable<Expression<Func<T, bool>>> list = Specifications.Select(x => x.ToExpression());

        ICollection<Expression> visitedBodies = [];

        foreach(var expr in list)
        {
            ParameterVisitor visitor = new(expr.Parameters[0], param);
            visitedBodies.Add(visitor.Visit(expr.Body));
        }

        Expression combined = visitedBodies.Aggregate(Expression.AndAlso) ?? Expression.Empty();

        return Expression.Lambda<Func<T, bool>>(combined, param);
    }
}