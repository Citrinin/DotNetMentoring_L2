using System;
using System.Linq;
using System.Linq.Expressions;

namespace Expressions_Task2
{
    public class MappingGenerator
    {
        public Mapper<TSource, TDestination> Generate<TSource, TDestination>()
        {
            var sourceParam = Expression.Parameter(typeof(TSource));

            var newExpression = Expression.New(typeof(TDestination));


            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            var list = sourceType.GetProperties()
                .Select(sourceProperty => destinationType.GetProperty(sourceProperty.Name))
                .Where(destinationProperty => destinationProperty != null)
                .Select(destinationProperty =>
                {
                    var call = Expression.Property(sourceParam, destinationProperty.Name);
                    return Expression.Bind(destinationProperty.GetSetMethod(), call);
                });

            var mapFunction =
                Expression.Lambda<Func<TSource, TDestination>>(
                    Expression.MemberInit(
                        Expression.New(typeof(TDestination)),
                        list),
                    sourceParam
                );

            Console.WriteLine(mapFunction);

            return new Mapper<TSource, TDestination>(mapFunction.Compile());
        }
    }
}