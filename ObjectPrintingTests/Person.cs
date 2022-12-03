using System;
using System.Reflection;

namespace ObjectPrintingTests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }

        public int IntField;

        public Person Parent { get; set; }
    }
}