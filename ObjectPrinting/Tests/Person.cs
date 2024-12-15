using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname;
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime Birthdate;
        public Person test;
    }
}