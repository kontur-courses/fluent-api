using FluentAssertions.Equivalency;
using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Person? Friend { get; set; }

        public List<Person>? Friends { get; set; }

        public Dictionary<int, Person>? Neighbours { get; set; }
    }
}