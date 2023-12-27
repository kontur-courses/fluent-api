using System;

namespace ObjectPrintingTests.TestHelpers
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public SubPerson SubPerson { get; set; }

        public string PublicField;
        private string privateField;
    }
}