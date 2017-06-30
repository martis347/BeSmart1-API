using Lunch.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Lunch.Host.Controllers
{
    [Route("api/[controller]")]
    public class DishesController: Controller
    {
        [HttpPost]
        public void SelectDishes(UserSelection selection)
        {
            
        }
    }
}