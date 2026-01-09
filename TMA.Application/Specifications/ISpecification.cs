using System.Linq.Expressions;

namespace TMA.Application.Specifications;

public interface ISpecification<T> 
{
    Expression<Func<T, bool>> ToExpression();
}