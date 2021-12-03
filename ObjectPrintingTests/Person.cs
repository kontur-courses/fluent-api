using System;
using System.Collections.Generic;

namespace ObjectPrintingTests
{
    internal class Person
    {
        public Guid Id { get; set; }
        public double[] Arr;
        public List<string> List;
        public Dictionary<int, string> Dict;
        public string Name { get; set; }
        public string Surname;
        public Person Parent;
        public double Height { get; set; }
        public int Age { get; set; }
    }
}