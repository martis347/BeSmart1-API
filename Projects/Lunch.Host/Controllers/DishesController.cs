using System.Threading.Tasks;
using Lunch.Domain;
using Lunch.Services.Dishes;
using Microsoft.AspNetCore.Mvc;

namespace Lunch.Host.Controllers
{
    [Route("api/[controller]")]
    public class DishesController: Controller
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
    }
}