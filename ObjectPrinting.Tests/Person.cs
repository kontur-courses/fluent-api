namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public string Email;
        public Person Parent { get; set; }
        public Person Child { get; set; }
    }
}