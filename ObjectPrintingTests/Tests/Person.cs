using System;

namespace ObjectPrintingTests.Tests
{
    public class Person
    {
        public Person Father { get; set; }
        public int Id2 = 0;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
    }
}