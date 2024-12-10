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
        public DateTime DateOfBirth { get; set; }
        public Dictionary<int, string> Addresses { get; set; }
        public List<Person> Children { get; set; }
        public Person Father { get; set; }
    }
}