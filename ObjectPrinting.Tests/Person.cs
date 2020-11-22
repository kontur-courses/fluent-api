using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public DateTime BirthDate;
        public Parent Parent;
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}