using System.Collections.Generic;

namespace Lunch.Domain
{
    public class Dish
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Count { get; set; }
        public string Category { get; set; }
        public DishType DishType { get; set; }
        public List<string> SideDishes { get; set; }
        public List<string> MainDishes { get; set; }
    }
}
