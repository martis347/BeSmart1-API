using System.Collections.Generic;
using System.Threading.Tasks;
using Lunch.Domain;
using Lunch.Services.Dishes;
using Microsoft.AspNetCore.Mvc;

namespace Lunch.Host.Controllers
{
    [Route("api/[controller]")]
    public class DishesController: AuthenticatedController
    {
        private readonly IDishesService _dishesService;
        
        public DishesController(IDishesService dishesService)
        {
            _dishesService = dishesService;
        }

        [HttpPut("{dayOfWeek}")]
        public async Task SelectDishes([FromBody] UserSelection selection, string dayOfWeek)
        {
            await _dishesService.SelectDishes(selection, dayOfWeek).ConfigureAwait(false);
        }

        [HttpGet("{dayOfWeek}/{username}")]
        public async Task<SelectedDishesResponse> GetSelectedDishes(string dayOfWeek, string username)
        {
            var result = await _dishesService.GetSelectedDishes(dayOfWeek, username).ConfigureAwait(false);

            return result;
        }
    }
}