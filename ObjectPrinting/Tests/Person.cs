using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }
        public Person? Parent { get; set; }
        public Person[] Friends { get; set; } = [];

        public Dictionary<int, string> SomeDictionary { get; set; } = new();
    }
}