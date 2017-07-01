using System.Collections.Generic;

namespace Lunch.Domain
{
    public class Provider
    {
        public Provider()
        {
            Dishes = new List<Dish>();
        }

        public List<Dish> Dishes { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}