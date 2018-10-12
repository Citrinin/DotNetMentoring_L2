using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithreading_Task1
{
    class Program
    {
        static void Main(string[] args)
        {
            var tasksList = new List<Task>();

            for (var i = 0; i < 100; i++)
            {
                tasksList.Add(PrinterTask(i));
            }

            Task.WaitAll(tasksList.ToArray());
            Console.WriteLine("Finish");
        }

        public static Task PrinterTask(int sequenceNumber)
        {
            return Task.Run(() =>
            {
                for (var j = 0; j < 1000; j++)
                {
                    Console.WriteLine($"Task #{sequenceNumber} – {j}");
                }
            });
        }
    }
}
