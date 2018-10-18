using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Await_Task1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the limit");


            //var limit = 800_000_000L;

            var oldTokenSource = new CancellationTokenSource();

            while (true)
            {
                var number = Console.ReadLine();
                if (!long.TryParse(number, out var limit))
                {
                    Console.WriteLine("Wrong input number. Assume start number is 0");
                }
                oldTokenSource.Cancel();
                var newTokenSource = new CancellationTokenSource();

                SumAsync(limit, newTokenSource.Token);
                oldTokenSource = newTokenSource;

            }
        }

        public static Task SumAsync(long limit, CancellationToken t)
        {
            return Task.Run(() =>
            {

                long result = 0;
                for (long i = 0; i <= limit; i++)
                {
                    if (t.IsCancellationRequested)
                    {
                        Console.WriteLine("Operation is stoped by cancellation token");
                        return;
                    }
                    result += i;
                }

                Console.WriteLine($"Sum of numbers from 0 to {limit} is equals {result}");
            }, t);
        }
    }
}
