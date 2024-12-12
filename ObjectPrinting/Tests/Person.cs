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
        public int Age { get; set; }
        
        public Person? OtherPerson { get; set; }
        public List<Person> Persons { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Surname, Height, Age);
        }
    }
}