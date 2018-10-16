using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithreading_Task2
{
    public static class ArrayPrinter
    {
        public static void PrintArray(this int[] array)
        {
            Console.WriteLine(String.Join(", ", array));
        }
    }
}
