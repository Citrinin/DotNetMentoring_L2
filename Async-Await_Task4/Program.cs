using System.Configuration;

namespace Async_Await_Task4
{
    class Program
    {
        static void Main(string[] args)
        {
            var conString = ConfigurationManager.ConnectionStrings["defaultConnection"]
                .ConnectionString;
        }
    }
}
