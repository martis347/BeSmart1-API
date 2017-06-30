namespace Lunch.Domain
{
    public class Person
    {
        public Person(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }
}