namespace Lunch.Domain
{
    public class UserSelection
    {
        public string Price { get; set; }
        public DishSelection MainDish { get; set; }
        public DishSelection SideDish { get; set; }
        public string UserName { get; set; }
    }

    public class DishSelection
    {
        public string Name { get; set; }
        public string Provider { get; set; }

    }
}