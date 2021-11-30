using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName;
        public double Height { get; set; }
        public int Age { get; set; }
        public Person Parent { get; set; }
    }

    public class Pet
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}