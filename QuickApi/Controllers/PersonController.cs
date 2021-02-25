using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;

namespace QuickApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        [HttpGet]
        [Route("CreateSome/{count}")]
        public async Task<ActionResult<List<Person>>> CreateSome(int count)
        {
            var faker = new Faker<Person>();
            
            faker.RuleFor(p => p.Id, f => Guid.NewGuid());
            faker.RuleFor(p => p.Name, f => f.Person.FullName);
            faker.RuleFor(p => p.DOB, f => f.Person.DateOfBirth);
            faker.RuleFor(p => p.Count, () => count);

            return Ok(faker.Generate(10).ToArray());
        }
    }

    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public int Count { get; set; }
    }
}
