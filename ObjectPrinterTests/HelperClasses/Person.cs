using System;

namespace ObjectPrinterTests.HelperClasses
{
    public class Person
    {
        public Person Parent { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public double Height { get; set; }

        public int Age { get; set; }
    }
}