using ObjectPrinting.Solved.Tests;
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

        public Person Parent { get; set; }
        
        public double[] Doubles = new double[2];

        public Dictionary<int, float> Dictionary = new Dictionary<int, float>()
        {
            { 1, 2.02f },
            { 3, 4.04f }
        };

        public SubPerson SubPerson { get; set; }
    }
}