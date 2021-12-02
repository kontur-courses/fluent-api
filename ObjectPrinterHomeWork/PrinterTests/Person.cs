using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id;
        public Person Father;
        public Person Son;
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age => DateTime.Now.Year - BirthDate.Year;
        public DateTime BirthDate { get; set; }
    }
}