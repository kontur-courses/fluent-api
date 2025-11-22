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
        public DateTime LastTimeOnline { get; set; }
        public Person Father { get; set; }
        public Person Child { get; set; }
        public string[] Tags { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }
    }
}