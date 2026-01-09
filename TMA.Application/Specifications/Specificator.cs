using System.Linq.Expressions;
using System.Reflection;

namespace TMA.Application.Specifications;

public static class Specificator<T>
{
    private static Expression<Func<T, bool>> MakeSpecificationTree()
    {
        List<Expression> parts = [];

        ParameterExpression param = Expression.Parameter(typeof(T), "p");

        foreach(PropertyInfo pi in param.Type.GetProperties())
        {

        }
    }
}