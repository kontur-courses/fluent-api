using System;

namespace ObjectPrintingTests
{
    internal class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname;
        public Person Parent;
        public double Height { get; set; }
        public int Age { get; set; }
    }
}