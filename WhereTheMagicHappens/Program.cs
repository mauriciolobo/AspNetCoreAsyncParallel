using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();

            app.Run();

            Console.ReadLine();
        }

    }

    internal class App
    {
        public void Run()
        {
            var startedOn = DateTime.UtcNow;
            var taskList = new ConcurrentBag<Task>();
            
            Parallel.For(1, 101, i =>
            {
                taskList.Add(Task.Run(() => GetPeopleAsync(i)));
            });

            Task.WaitAll(taskList.ToArray());

            var delta = DateTime.UtcNow - startedOn;

            Console.WriteLine($"Total tasks completed: {taskList.Count} in {delta.TotalMilliseconds}ms");


            PlayWithDB();

        }

        private void PlayWithDB()
        {
            Console.WriteLine("----DB press <enter>------");
            Console.ReadLine();

            var db = new MyDbContext();
            var totalCounts = db.People.GroupBy(p => p.Count).Count();
            Console.WriteLine($"Total counts: " + totalCounts);
            foreach (var i in db.People.GroupBy(p => p.Count).Select(s=>s.Key).OrderBy(s=>s))
            {
                Console.WriteLine($"Total count {i}: " + db.People.Count(p => p.Count == i));
            }
            Console.WriteLine($"Total Register: " + db.People.Count());
        }

        private async Task GetPeopleAsync(int idCounter)
        {
            GetPeople(idCounter);
        }

        private void GetPeople(int idCounter)
        {
            var result = GetFromClient(idCounter);
            Parallel.ForEach(result, p =>
            {
                Console.WriteLine($"Person: ({p.Count}) {p.Name} - {p.DOB.ToShortDateString()}");
            });
            var _db = new MyDbContext();
            _db.People.AddRange(result);
            _db.SaveChanges();
        }

        private List<Person> GetFromClient(int count)
        {
            Console.WriteLine("Getting data from client count: " + count);
            var client = new HttpClient();
            var httpResult = client.GetAsync("https://localhost:44302/api/Person/CreateSome/" + (count + 1)).Result;
            var result = httpResult.Content.ReadAsStringAsync().Result;

            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Person>>(result);
        }

        public class Person
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime DOB { get; set; }
            public int Count { get; set; }
        }

        public class MyDbContext : DbContext
        {
            public DbSet<Person> People { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Person>().HasKey(k => k.Id);
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase("InMemory");
            }
        }
    }
}
