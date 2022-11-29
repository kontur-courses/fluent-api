using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Person Child;
        public Car Car;
        public Guid Id { get; set; }
        public string Name { get; set; } = "a";
        public string Surname { get; set; }
        public double Height { get; set; }
        public float Width { get; set; }
        public int Age { get; set; }
        public float Mass;
    }
}