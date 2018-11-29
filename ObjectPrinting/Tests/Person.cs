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
        public Person Child { get; set; }

        public Person()
        {
        }

        public Person(Person parent)
        {
            Child = parent;
        }
    }

    public class PersonWithCollections
    {
        public ICollection<Person> Childs { get; set; }

        public ICollection<double> Heights { get; set; }
    }
}