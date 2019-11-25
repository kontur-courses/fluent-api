using System;
using System.Runtime.Serialization;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public Name Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
    }

   
}