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
        public int Money;
        public string Country { get; set; }
        public House House { get; set; }
        public string[] Languages { get; set; }
        public Dictionary<string, int> Currency { get; set; }
    }
}