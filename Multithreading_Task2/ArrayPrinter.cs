using System;

namespace Multithreading_Task2
{
    public static class ArrayPrinter
    {
        public static void PrintArray(this int[] array)
        {
            Console.WriteLine(string.Join(", ", array));
        }
    }
}
