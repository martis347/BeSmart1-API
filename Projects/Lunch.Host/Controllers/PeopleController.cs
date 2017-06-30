using System.Collections.Generic;
using Lunch.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Lunch.Host.Controllers
{
    [Route("api/[controller]")]
    public class PeopleController : Controller
    {
        [HttpGet]
        public IList<Person> Get()
        {
            return new List<Person>
            {
                new Person("Greg M."),
                new Person("Betty P."),
                new Person("Johnny T."),
            };
        }
    }
}
