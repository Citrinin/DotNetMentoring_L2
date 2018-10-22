using System;

namespace Expressions_Task2
{
    class Program
    {
        static void Main(string[] args)
        {

            var mapGenerator = new MappingGenerator();
            var mapper = mapGenerator.Generate<Foo, Bar>();

            var foo = new Foo
            {
                IntProperty = 10,
                StringProperty = "123",
                DoubleProperty = 10f
            };

            var bar = mapper.Map(foo);

            Console.WriteLine(foo);
            Console.WriteLine(bar);

            Console.ReadKey();
        }
    }
}
