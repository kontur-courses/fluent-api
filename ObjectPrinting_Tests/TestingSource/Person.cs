using System;

namespace ObjectPrintingTests.TestingSource
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; init; }
        public double Height { get; init; }
        public int Age { get; init; }
    }
}