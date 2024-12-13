using System;
using System.Collections.Generic;
using System.Drawing;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public List<Person> Children { get; set; }
        public Person Father { get; set; }
        public DateTime DateOfBirth;
        public Dictionary<int, string> Addresses;
    }
}