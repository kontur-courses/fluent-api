using System;
using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public int weight;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Person Parent { get; set; }
        public Person Child { get; set; }
        
        public ICollection<string> Items { get; set; }
        public IDictionary<int,string> SignificantEvents { get; set; }
    }
}