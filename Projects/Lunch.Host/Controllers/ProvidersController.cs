using System.Collections.Generic;
using Lunch.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Lunch.Host.Controllers
{
    [Route("api/[controller]")]
    public class ProvidersController: Controller
    {
        [HttpGet]
        public IList<Provider> Get()
        {
            return new List<Provider>
            {
                new Provider(),
                new Provider(),
                new Provider()
            };
        }

    }
}