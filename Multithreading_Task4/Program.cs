using System;
using System.Threading;

namespace Multithreading_Task4
{
    class Program
    {
        static void Main(string[] args)
        {
            var thread = new Thread(ThreadCallback);
            thread.Start(new ThreadParameter { Counter = 10, Number = 100 });
        }

        static void ThreadCallback(object param)
        {
            var parameter = (ThreadParameter)param;
            Console.WriteLine($"Current thread {Environment.CurrentManagedThreadId}, thread number {parameter.Counter}, current number {parameter.Number}");
            parameter.Counter--;
            parameter.Number--;
            if (parameter.Counter > 0)
            {
                var thread = new Thread(ThreadCallback);
                thread.Start(parameter);
                thread.Join();
            }

            Console.WriteLine($"Current thread {Environment.CurrentManagedThreadId}");
        }

        class ThreadParameter
        {
            public int Counter { get; set; }
            public int Number { get; set; }
        }
    }
}
