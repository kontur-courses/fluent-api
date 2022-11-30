using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id;
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public double Double { get; set; }
        public Person Parent { get; set; }
    }
}