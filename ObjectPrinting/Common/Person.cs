using System;
using System.Collections.Generic;

namespace ObjectPrinting.Common
{
    public class Person
    {
        public Guid Id { get; set; }
        public FullName FullName { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }

        public Person[] Parents { get; set; }
        
        public Dictionary<string, string> Documents { get; set; }
    }
}