using System;

namespace ObjectPrintingTests.TestModels
{
    public class Person1
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public Person Friend { get; set; }
    }
}