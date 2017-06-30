using System.Collections.Generic;

namespace Lunch.Domain
{
    public class Provider
    {
        public List<Dish> Dishes { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}