namespace ObjectPrintingTests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public double Weight { get; set; }
        public List<Person> Friends { get; set; }
        public Person[] Parents { get; set; }
        public Dictionary<int, string> SomeDictionary { get; set; }
    }
}
