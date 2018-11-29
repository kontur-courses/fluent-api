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
    }

    public class Token
    {
        public int Value { get; set; }
        public Token Next { get; set; }
    }
}