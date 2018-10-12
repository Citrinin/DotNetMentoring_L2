using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multithreading_Task6
{
    class Program
    {
        static void Main(string[] args)
        {
            var syncMutex = new Mutex();
            var sharedList = new List<int>();

            var taskWriter = Task.Run(() =>
            {
                while (true)
                {
                    syncMutex.WaitOne();
                    sharedList.AddRange(Enumerable.Range(0, 10));
                    syncMutex.ReleaseMutex();
                }
            });

            var taskReader = Task.Run(() =>
            {
                while (true)
                {
                    syncMutex.WaitOne();
                    Console.WriteLine("Begining of printing");
                    sharedList.ForEach(Console.WriteLine);
                    Console.WriteLine($"End of printing. Total items {sharedList.Count}");
                    Thread.Sleep(1000);
                    syncMutex.ReleaseMutex();
                }
            });

            Thread.Sleep(10000);
        }
    }
}
