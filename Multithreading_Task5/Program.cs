using System;
using System.Threading;

namespace Multithreading_Task5
{
    class Program
    {
        private static Semaphore _syncSemaphore = new Semaphore(0, 1);

        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(ThreadCallback, new ThreadParameter { Counter = 10, Number = 100 });
            _syncSemaphore.WaitOne();
        }

        static void ThreadCallback(object param)
        {
            var parameter = (ThreadParameter)param;
            Console.WriteLine($"Current thread {Environment.CurrentManagedThreadId}, thread number {parameter.Counter}, current number {parameter.Number}");
            parameter.Counter--;
            parameter.Number--;
            if (parameter.Counter > 0)
            {
                ThreadPool.QueueUserWorkItem(ThreadCallback, parameter);
                _syncSemaphore.WaitOne();
            }
            _syncSemaphore.Release();
        }

        class ThreadParameter
        {
            public int Counter { get; set; }
            public int Number { get; set; }
        }
    }
}
