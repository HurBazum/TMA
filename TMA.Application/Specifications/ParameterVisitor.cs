using System.Linq.Expressions;

namespace TMA.Application.Specifications;

internal class ParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter) : ExpressionVisitor
{
    public ParameterExpression OldParameter { get; } = oldParameter;
    public ParameterExpression NewParameter { get; } = newParameter;

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if(node == OldParameter)
        {
            return NewParameter;
        }
        else
        {
            return base.VisitParameter(node);
        }
    }
}