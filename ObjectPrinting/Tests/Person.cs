using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class PersonWithList
    {
        public List<int> List { get; set; }
        public int Number { get; set; } = 12378;
        public Dictionary<string, float> Dict { get; set; }
        public Queue<Guid> Guids { get; set; }
    }
    
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public char Index { get; set; }
        public int Age { get; set; }
        public DateTime Birthday { get; set; }
        public Person Parent { get; set; }
    }
}