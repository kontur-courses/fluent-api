using System;
using System.Collections.Generic;

namespace ObjectPrintingTests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public float Weight { get; set; }
        public int Age { get; set; }

        public Person[] Parents { get; set; }

        public List<Person> Children { get; set; }

        public Dictionary<string, string> OtherInfo { get; set; }
    }
}