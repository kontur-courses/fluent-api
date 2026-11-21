namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = null!;
        public string Secondname { get; set; } = null!;
        public string? Surname { get; set; }

        public int Age { get; set; }
        public double HeightInMeters { get; set; }
        public double WeightInKilograms { get; set; }

        public Person? Father { get; set; }
        public Person? Mother { get; set; }
        public List<Person>? Children { get; set; }
    }
}