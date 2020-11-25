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
        public string Surname;
        public Person Friend { get; set; }
        public Person Friend2 { get; set; }
        public List<int> Codes;
    }
}