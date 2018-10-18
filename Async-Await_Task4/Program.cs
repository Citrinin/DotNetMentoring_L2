using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Async_Await_Task4
{
    class Program
    {
        static void Main(string[] args)
        {
            Task4Async();
            Console.ReadKey();
        }

        public static async Task Task4Async()
        {
            var personRepo = new PersonRepository();
            var persons = await personRepo.GetAllAsync();
            PrintPersons(persons, "Getting persons from database");

            var personToAdd = new Person {FirstName = "Anton", LastName = "Antonov", Age = 24};
            await personRepo.AddAsync(personToAdd);
            persons = await personRepo.GetAllAsync();
            PrintPersons(persons, "Adding Anton Antonov to database");

            personToAdd.FirstName = "Kirill";
            if (await personRepo.UpdateAsync(personToAdd, nameof(personToAdd.FirstName)))
            {
                persons = await personRepo.GetAllAsync();
                PrintPersons(persons, "Updating existing person name of Andrey to Kirill in database");
            }
            else
            {
                Console.WriteLine("Unsuccessful updating");
            }


            if (await personRepo.DeleteAsync(1))
            {
                persons = await personRepo.GetAllAsync();
                PrintPersons(persons, "Deleting person with Id=1 from database");
            }
            else
            {
                Console.WriteLine("Unsuccessful deleting");
            }

        }

        public static void PrintPersons(IEnumerable<Person> persons, string note)
        {
            Console.WriteLine(note);
            Console.WriteLine(string.Join("\n", persons.Select(p => $"{p.Id}\t{p.FirstName}\t{p.LastName}\t{p.Age}")));
        }
    }
}