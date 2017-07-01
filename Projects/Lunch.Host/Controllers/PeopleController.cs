using System.Collections.Generic;
using System.Threading;
using Lunch.Domain;
using Lunch.Sheets.Client;
using Microsoft.AspNetCore.Mvc;

namespace Lunch.Host.Controllers
{
    [Route("api/[controller]")]
    public class PeopleController : Controller
    {
        private readonly ISheetsClient _client;
        public PeopleController(ISheetsClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IList<Person> Get()
        {
            var accessToken = Request.Headers["access_token"];
            
            _client.GetSheetData("", "", "");
            return new List<Person>
            {
                new Person("Greg M."),
                new Person("Betty P."),
                new Person("Johnny T."),
            };
        }
    }
}
