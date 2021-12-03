using System;
using System.Collections.Generic;

namespace ObjectPrinting.Solved.Tests
{
    public class Person
    {
        //public List<Person> AnotherPersons { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public int Health { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Person Parent { get; set; }
    }
}