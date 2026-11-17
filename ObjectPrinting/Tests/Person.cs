using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Height { get; set; }
        public int Age { get; set; }
        public Guid ExternalId = Guid.Empty;
        public int Score = 42;
    }
}