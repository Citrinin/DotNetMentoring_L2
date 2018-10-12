using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Multithreading_Task3
{
    class Program
    {
        private static readonly Random _random = new Random();
        private static Stopwatch _timer;

        static void Main(string[] args)
        {
            var matrixHeight = 1000;
            var matrixWidth = 1500;

            var firstMatrix = GetRandomArray(matrixHeight, matrixWidth);
            var secondMatrix = GetRandomArray(matrixWidth, matrixHeight);

            //PrintArray(firstMatrix);
            //PrintArray(secondMatrix);

            var resultMatrix = new int[matrixHeight, matrixHeight];

            StartTimer("Parallel process");

            Parallel.For(0, firstMatrix.GetLength(0), (i) =>
            {
                Parallel.For(0, firstMatrix.GetLength(0), (j) =>
                {
                    resultMatrix[i, j] = 0;
                    Parallel.For(0, firstMatrix.GetLength(1), (k) => resultMatrix[i, j] += firstMatrix[i, k] * secondMatrix[k, j]);
                });
            });
            //PrintArray(result);
            EndTimer();

            StartTimer("For loop process");

            for (var i = 0; i < firstMatrix.GetLength(0); i++)
            {
                for (var j = 0; j < firstMatrix.GetLength(0); j++)
                {
                    resultMatrix[i, j] = 0;
                    for (var k = 0; k < firstMatrix.GetLength(1); k++)
                    {
                        resultMatrix[i, j] += firstMatrix[i, k] * secondMatrix[k, j];
                    }
                }
            }

            //PrintArray(result);
            EndTimer();
        }

        private static int[,] GetRandomArray(int height, int width)
        {
            var array = new int[height, width];
            Parallel.For(0, height, i => Parallel.For(0, width, j => array[i, j] = GetRandomInt()));

            return array;
        }

        private static int GetRandomInt()
        {
            return _random.Next(-100, 100);
        }

        private static void PrintArray(int[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    Console.Write($"{array[i, j]}\t");
                }

                Console.WriteLine();
            }
        }

        private static void StartTimer(string taskName)
        {
            Console.WriteLine($"Starting the {taskName}");
            _timer = Stopwatch.StartNew();
        }

        private static void EndTimer()
        {
            _timer.Stop();
            Console.WriteLine($"Execution takes {_timer.ElapsedMilliseconds}");
        }
    }
}
