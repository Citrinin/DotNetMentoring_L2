using System;

namespace Expressions_Task2
{
    public class Mapper<TSource, TDestination>
    {
        Func<TSource, TDestination> mapFunction;

        internal Mapper(Func<TSource, TDestination> func)
        {
            mapFunction = func;
        }

        public TDestination Map(TSource source)
        {
            return mapFunction(source);
        }
    }
}