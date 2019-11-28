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
        public List<int> Score { get; set; }
        public Dictionary<Person, string> Dictionary { get; set; } 
    }
}