using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }

        public bool BoolField;
        public Person Friend;
        public double[] Numbers;
        public Dictionary<string, double> Dict;

        public static Person CreatePerson()
        {
            return new  Person {Name = "Alex", Age = 20, Height = 170.5, Weight = 55, BoolField = true};
        }
    }
}