using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Multithreading_Task7
{
    class Program
    {
        //        7.	Create a Task and attach continuations to it according to the following criteria:
        //    a.Continuation task should be executed regardless of the result of the parent task.
        //    b.Continuation task should be executed when the parent task finished without success.
        //    c.Continuation task should be executed when the parent task would be finished with fail and parent task thread should be reused for continuation
        //    d.Continuation task should be executed outside of the thread pool when the parent task would be cancelled
        //    Demonstrate the work of the each case with console utility.

        static void Main(string[] args)
        {

            var taskASuccess = Task.Run(() => { Thread.Sleep(1000); })
                .ContinueWith(result => Console.WriteLine("Continuation of taskA success"), TaskContinuationOptions.None);
            taskASuccess.Wait();

            var taskAFail = Task.Run(() => { Thread.Sleep(1000); throw new Exception(":("); })
                .ContinueWith(result =>
                    {
                        Console.WriteLine("Continuation of taskA exception");
                        Console.WriteLine(result.Exception?.Message);
                    }, TaskContinuationOptions.None);
            taskAFail.Wait();

            var taskBSuccess = Task.Run(() => { Thread.Sleep(1000); });
            taskBSuccess
                .ContinueWith(result =>
                    {
                        Console.WriteLine("Continuation of taskB success");
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
            taskBSuccess.Wait();

            var taskBFail = Task.Run(() => { Thread.Sleep(1000); throw new Exception(":("); }).ContinueWith(result =>
            {
                Console.WriteLine("Continuation of taskB exception");
                Console.WriteLine($"error: {result.Exception?.Message}");
            }, TaskContinuationOptions.OnlyOnFaulted);
            taskBFail.Wait();



            var taskC = Task.Run(() =>
            {
                Thread.Sleep(1000);
                Console.WriteLine($"thread ID {Thread.CurrentThread.ManagedThreadId}");
                throw new Exception(":(");
            }).ContinueWith(
                result =>
                {
                    Console.WriteLine(
                        $"Continuation of taskC exception, thread ID {Thread.CurrentThread.ManagedThreadId}");
                }, TaskContinuationOptions.ExecuteSynchronously & TaskContinuationOptions.OnlyOnFaulted);
            taskC.Wait();

            var token = new CancellationToken(true);
            var taskD = Task.Run((() => { Thread.Sleep(1000); }), token).ContinueWith(
                (result) =>
                {
                    Console.WriteLine(
                        $"Continuation of taskD, thread pool = {Thread.CurrentThread.IsThreadPoolThread}");
                }, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled , new CustomTaskScheduler());

            taskD.Wait();
        }
    }

    public class CustomTaskScheduler : TaskScheduler
    {
        private ConcurrentQueue<Task> tasksCollection = new ConcurrentQueue<Task>();

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return tasksCollection;
        }

        protected override void QueueTask(Task task)
        {
            (new Thread(() => TryExecuteTask(task))).Start();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }
    }

}
