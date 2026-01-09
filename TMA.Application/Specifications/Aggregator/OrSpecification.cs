using System.Linq.Expressions;

namespace TMA.Application.Specifications.Aggregator;

internal class OrSpecification<T>(params ISpecification<T>[] specifications) : ISpecification<T>
{
    public ISpecification<T>[] Specifications { get; } = specifications;

    public Expression<Func<T, bool>> ToExpression()
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "t");

        IEnumerable<Expression<Func<T, bool>>> list = Specifications.Select(x => x.ToExpression());

        ICollection<Expression> visitedBodies = [];

        foreach(Expression<Func<T, bool>> expr in list)
        {
            ParameterVisitor visitor = new(expr.Parameters[0], param);
            visitedBodies.Add(visitor.Visit(expr.Body));
        }

        var combined = visitedBodies.Aggregate(Expression.OrElse) ?? Expression.Empty();

        return Expression.Lambda<Func<T, bool>>(combined, param);
    }
}