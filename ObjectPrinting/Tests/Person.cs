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

        public Person Dad { get; set; }

        public int[] ArmsLenght { get; set; }
        public List<string> FriendsNames { get; set; }
        public Dictionary<string, int> Awards { get; set; }
    }
}