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
        public int Weight { get; set; }
        public List<Person> Children { get; }
        public Dictionary<string, int> CustomDict { get; }
        public Person Parent { get; set; }

        public Person()
        {
            Children = new List<Person>();
            CustomDict = new Dictionary<string, int>();
        }
    }
}