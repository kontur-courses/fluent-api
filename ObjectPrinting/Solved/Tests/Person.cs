using System;

namespace ObjectPrinting.Solved.Tests
{
    public class Person
    {
        public DateTime BirthDate;
        public Parent Parent;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public int Weight { get; set; }
    }
}