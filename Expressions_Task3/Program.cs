using System;
using System.Configuration;
using System.Linq;
using Expressions_Task3.E3SClient;
using Expressions_Task3.E3SClient.Entities;

namespace Expressions_Task3
{
    class Program
    {
        static void Main(string[] args)
        {
            WithoutProvider();
            WithoutProviderNonGeneric();
            WithProvider();
            WithProviderSwitchPosition();
            WithProviderStartsWith();
            WithProviderEndsWith();
            WithProviderContains();

            Console.ReadKey();
        }

        public static void WithoutProvider()
        {
            var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
            var res = client.SearchFTS<EmployeeEntity>("fullName:(Ali* Kaz*)", 0, 1);

            foreach (var emp in res)
            {
                PrintEmployee(emp);
            }
        }

        public static void WithoutProviderNonGeneric()
        {
            var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
            var res = client.SearchFTS(typeof(EmployeeEntity), "workstation:(EPBYMINW7893)", 0, 10);

            foreach (var emp in res.OfType<EmployeeEntity>())
            {
                PrintEmployee(emp);
            }
        }

        public static void WithProvider()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.workstation == "EPRUIZHW0249"))
            {
                PrintEmployee(emp);
            }
        }

        public static void WithProviderSwitchPosition()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => "EPRUIZHW0249" == e.workstation ))
            {
                PrintEmployee(emp);
            }
        }

        public static void WithProviderStartsWith()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.workstation.StartsWith("EPRUIZH")))
            {
                PrintEmployee(emp);
            }
        }

        public static void WithProviderEndsWith()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.nativename.EndsWith("зькина Екатерина Евгеньевна")))
            {
                PrintEmployee(emp);
            }
        }

        public static void WithProviderContains()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.nativename.Contains("зькина Екатерина Евгень")))
            {
                PrintEmployee(emp);
            }
        }

        public static void PrintEmployee(EmployeeEntity emp)
        {
            Console.WriteLine($"{emp.nativename} {emp.country[0]} {emp.city[0]} {emp.office}");
        }
    }
}
