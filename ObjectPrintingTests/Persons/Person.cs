using System;

namespace ObjectPrintingTests.Persons
{
    public class Person
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = "Alex";
        public double Height { get; set; } = 2.1;
        public int Age { get; set; } = 19;
    }
}