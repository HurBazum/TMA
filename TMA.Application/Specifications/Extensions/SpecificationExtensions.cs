using TMA.Application.Specifications.Aggregator;

namespace TMA.Application.Specifications.Extensions;

public static class SpecificationExtensions
{
    public static ISpecification<T> And<T>(this ISpecification<T> spec, params ISpecification<T>[] others) => new AndSpecification<T>([.. others.Union([spec])]);
}