using System;

namespace KeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var kc = new KeyCheck();

            Console.WriteLine(kc.GenerateKey());
        }
    }
}
