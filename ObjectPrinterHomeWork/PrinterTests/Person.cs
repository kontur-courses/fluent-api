using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public readonly Guid Id;
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age => DateTime.Now.Year - BirthDate.Year;

        public DateTime BirthDate { get; set; }
    }
}