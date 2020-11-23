using System;
using System.Collections.Generic;

namespace ObjectPrinting.Solved.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Person AlterEgo { get; set; }
        public int[] Array { get; set; }
        public List<double> List { get; set; }
        public Dictionary<string, string> Dict { get; set; }
        public List<List<int>> ListOfList { get; set; }
    }
}