using System;
using System.Linq;
using System.Threading.Tasks;

namespace Multithreading_Task2
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            var task = Task.Run(() =>
                {
                    Console.WriteLine("First thread");
                    var array = new int[10];
                    for (var i = 0; i < 10; i++)
                    {
                        array[i] = random.Next(0, 1000);
                        Console.WriteLine(array[i]);

                    }
                    return array;
                })
                .ContinueWith(randomArray =>
                {
                    Console.WriteLine("Second thread");

                    var randomMultiplyer = random.Next(0, 1000);
                    Console.WriteLine($"Random multiplyer {randomMultiplyer}");

                    var multiplyedArray = randomArray.Result;
                    for (var i = 0; i < multiplyedArray.Length; i++)
                    {
                        multiplyedArray[i] *= randomMultiplyer;
                        Console.WriteLine(multiplyedArray[i]);
                    }
                    return multiplyedArray;
                }).ContinueWith(multiplyedArray =>
                {
                    Console.WriteLine("Third thread");
                    var arrayToSort = multiplyedArray.Result;
                    var sortedArray = arrayToSort.ToList();
                    sortedArray.Sort();
                    foreach (var i in sortedArray)
                    {
                        Console.WriteLine(i);
                    }
                    return sortedArray;
                }).ContinueWith(result =>
                {
                    Console.WriteLine("Fourth thread");
                    var average = result.Result.Average();
                    Console.WriteLine($"Average value equals {average}");
                });

            task.Wait();
        }
    }
}
