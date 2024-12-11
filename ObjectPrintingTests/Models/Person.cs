namespace ObjectPrintingTests.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
    }

    public class Child : Person
    {
        public Person Parent { get; set; }
    }

    public class SkilledPerson : Person
    {
        public Dictionary<string, int> Skills { get; set; }
    }
}