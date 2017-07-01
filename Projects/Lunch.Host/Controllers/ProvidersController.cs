using Lunch.Domain;
using Lunch.Services.Providers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lunch.Host.Controllers
{
    [Route("api/[controller]")]
    public class ProvidersController: Controller
    {
        private readonly IProvidersService _providers;
        public ProvidersController(IProvidersService providers)
        {
            _providers = providers;
        }
        
        [HttpGet]
        public async Task<IList<Provider>> Get()
        {
            IList<Provider> result;
            
            try
            {
                result = await _providers.GetProviders(DateTime.MaxValue).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return result;
        }
    }
}