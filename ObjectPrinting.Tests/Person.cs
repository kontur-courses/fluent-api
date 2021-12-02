using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public string LastName;
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Person Parent { get; set; }
    }
}