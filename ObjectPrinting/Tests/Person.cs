using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Dictionary<int, string> Pets { get; set; }
    }
}