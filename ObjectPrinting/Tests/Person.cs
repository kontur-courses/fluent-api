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
        
        public Person[] Relatives { get; set; }
        
        public Dictionary<Person, Person> Persons { get; set; }
        
        public int Field = 1;

        public string FamilyName = "abc";
    }
}