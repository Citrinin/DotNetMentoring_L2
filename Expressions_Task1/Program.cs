using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Expressions_Task1
{
    class Program
    {
        static void Main(string[] args)
        {

            Expression<Func<int, int>> source_exp = (a) => a + (a + 1) * (a + 5) * (a - 1) * (1 + a) * (1 - a);
            var result_exp = new IncrementDecrementTransform().VisitAndConvert(source_exp, "");

            Console.WriteLine(source_exp + " " + source_exp.Compile().Invoke(2));
            Console.WriteLine(result_exp + " " + result_exp.Compile().Invoke(2));

            Expression<Func<int, int, int>> task2_source_exp = (a, b) => b + (a + 1) * (a + 5) * (b - 1) * (b - a);
            var task2_result_exp = new ReplaceParamToConstantTransform()
                .VisitAndReplace(task2_source_exp, new Dictionary<string, int> {
                    { "a", 1 },
                    { "b", 2}
                });
            Console.WriteLine(task2_result_exp + " " + task2_result_exp.Compile().Invoke(0, 0));
        }
    }
}
