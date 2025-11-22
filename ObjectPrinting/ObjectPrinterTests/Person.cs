using System;
using System.Collections.Generic;

namespace ObjectPrinting.ObjectPrinterTests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        
        public int IQ { get; set; }
        
        public DateTime BirthDate {get; set;}
        public int[] Scores { get; set; }

        public List<string> Tags { get; set; }

        public Dictionary<string, int> Ratings { get; set; }

        public Person? Friend { get; set; }
    }
}