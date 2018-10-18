using System;
using System.Threading;

namespace Async_Await_Task2
{
    public class UriWrapper
    {
        public Uri UriAddress { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
    }
}
