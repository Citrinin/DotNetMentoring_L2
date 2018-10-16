using System;
using System.Threading;

namespace Multithreading_Task5
{
    class Program
    {
        private static Semaphore _syncSemaphore = new Semaphore(1, 1);

        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(ThreadCallback, new ThreadParameter { Counter = 10, Number = 100 });
            Console.ReadKey();
        }

        static void ThreadCallback(object param)
        {
            var parameter = (ThreadParameter)param;
            Console.WriteLine($"Current thread {Environment.CurrentManagedThreadId}, thread number {parameter.Counter}, current number {parameter.Number}");

            _syncSemaphore.WaitOne();
            parameter.Counter--;
            parameter.Number--;
            if (parameter.Counter > 0)
            {
                ThreadPool.QueueUserWorkItem(ThreadCallback, parameter);
            }
            _syncSemaphore.Release();
            Console.WriteLine($"Current thread {Environment.CurrentManagedThreadId}");
        }

        class ThreadParameter
        {
            public int Counter { get; set; }
            public int Number { get; set; }
        }
    }
}
