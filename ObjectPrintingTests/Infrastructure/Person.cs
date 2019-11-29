using System;

namespace ObjectPrintingTests.Infrastructure
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        
        public Person[] Friends { get; set; }

        private int budget;
    }
}