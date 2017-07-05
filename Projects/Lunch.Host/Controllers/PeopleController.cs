using Lunch.Domain;
using Lunch.Services.People;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lunch.Host.Controllers
{
    [Route("api/[controller]")]
    public class PeopleController : Controller
    {
        private readonly IPeopleService _peopleService;
        public PeopleController(IPeopleService peopleService)
        {
            _peopleService = peopleService;
        }

        [HttpGet]
        public async Task<IList<Person>> Get()
        {
            var result = await _peopleService.GetPeople().ConfigureAwait(false);

            return result;
        }
    }
}
