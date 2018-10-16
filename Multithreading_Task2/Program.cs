using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multithreading_Task2
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            var task = Task.Run(() =>
                {
                    Console.WriteLine("\nFirst thread");

                    var array = Enumerable.Range(0, 10).Select(r => random.Next(0, 1000)).ToArray();
                    array.PrintArray();
                    return array;
                })
                .ContinueWith(randomArray =>
                {
                    Console.WriteLine("\nSecond thread");

                    var randomMultiplyer = random.Next(0, 1000);
                    Console.WriteLine($"Random multiplyer {randomMultiplyer}");

                    var multiplyedArray = randomArray.Result.Select(i => i * randomMultiplyer).ToArray();
                    multiplyedArray.PrintArray();

                    return multiplyedArray;
                }).ContinueWith(multiplyedArray =>
                {
                    Console.WriteLine("\nThird thread");

                    var arrayToSort = multiplyedArray.Result.ToList();
                    arrayToSort.Sort();
                    var sortedArray = arrayToSort.ToArray();
                    sortedArray.PrintArray();

                    return sortedArray;
                }).ContinueWith(result =>
                {
                    Console.WriteLine("\nFourth thread");

                    var average = result.Result.Average();
                    Console.WriteLine($"Average value equals {average}");
                });

            task.Wait();
        }
    }
}
