using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Enumerics
    {
        public Array A { get; set; } = new int[4];

        public List<string> L { get; set; } = new List<string>
        {
            "Alex",
            "Vovan",
            "Peter"
        };

        public Dictionary<Person, Operation> D { get; set; } = new Dictionary<Person, Operation>
        {
            {new Person {Name = "Anna"}, new Operation {Operator1 = 1}},
            {new Person {Name = "Winona"}, new Operation {Operator1 = 3}}
        };
    }
}