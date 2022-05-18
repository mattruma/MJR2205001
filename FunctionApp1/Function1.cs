using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName(nameof(Function1))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            Randomizer.Seed = new Random(8675309);

            log.LogInformation("C# HTTP trigger function processed a request.");

            int page = 1;

            int.TryParse(req.Query["page"], out page);

            int pageSize = 20;

            int.TryParse(req.Query["pageSize"], out pageSize);

            var personFaker = new Faker<Person>()
                 .RuleFor(o => o.Name, f => f.Person.FullName)
                 .RuleFor(o => o.Phone, f => f.Person.Phone)
                 .RuleFor(o => o.Email, f => f.Person.Email)
                 .RuleFor(o => o.Bio, f => f.Lorem.Paragraphs(3));

            var people = new List<Person>();

            var start = (page - 1) * pageSize;
            var end = ((page - 1) * pageSize) + pageSize;

            if (end > 100) end = 100;

            for (var i = start; i < end; i++)
            {
                var person =
                    personFaker.Generate();

                person.Id = i + 1;

                var toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(person.Bio);

                person.Bio = Convert.ToBase64String(toEncodeAsBytes);

                people.Add(person);
            }

            return new OkObjectResult(people);
        }
    }
}
