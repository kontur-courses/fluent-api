namespace Tests.TestEntities
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double Height { get; set; }
        public int Age { get; set; }
    }
}