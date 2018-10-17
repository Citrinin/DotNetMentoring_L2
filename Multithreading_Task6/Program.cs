using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Multithreading_Task6
{
    class Program
    {
        static void Main(string[] args)
        {
            var readAutoResetEvent = new AutoResetEvent(false);
            var writeAutoResetEvent = new AutoResetEvent(true);
            var sharedList = new List<int>();

            var taskWriter = Task.Run(() =>
            {
                while (true)
                {
                    writeAutoResetEvent.WaitOne();
                    sharedList.AddRange(Enumerable.Range(0, 10));
                    readAutoResetEvent.Set();
                }
            });

            var taskReader = Task.Run(() =>
            {
                while (true)
                {
                    readAutoResetEvent.WaitOne();
                    Console.WriteLine("Begining of printing");
                    sharedList.ForEach(Console.WriteLine);
                    Console.WriteLine($"End of printing. Total items {sharedList.Count}");
                    Thread.Sleep(2000);
                    writeAutoResetEvent.Set();
                }
            });

            Console.ReadKey();
        }
    }
}
