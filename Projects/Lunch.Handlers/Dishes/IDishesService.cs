using System.Threading.Tasks;
using Lunch.Domain;

namespace Lunch.Services.Dishes
{
    public interface IDishesService
    {
        Task SelectDishes(UserSelection selection, string dayOfWeek);
        Task<SelectedDishesResponse> GetSelectedDishes(string dayOfWeek, string username);
    }
}