namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string SecondName { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public int[]? FavoriteNumbers { get; set; }
        public List<Person>? Familiars { get; set; }
        public Dictionary<string, string>? PetNames { get; set; }
        public Person? BestFriend { get; set; }
    }
}