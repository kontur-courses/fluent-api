using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id;
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime BirthDay { get; set; }
        public Person Spouse { get; set; }
        public List<Person> Family;
    }
}