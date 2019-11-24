using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public double Weight { get; set; }
        public Person Mother { get; set; }
        public Person Father { get; set; }
    }
}