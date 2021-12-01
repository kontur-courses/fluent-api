using System;

namespace ObjectPrintingTests
{
    public class Person
    {
        public Guid Id { get; set; }
        public FullName FullName { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }

        public Person AnotherPerson { get; set; }

        public string Field = "Public";
        private string privateField = "private";
    }

    public class FullName
    {
        public string Name { get; set; }
        public string Surname { get; set; }

        public int SpecialField = 92;

        public FullName(string name, string surname)
        {
            Name = name;
            Surname = surname;
        }
    }
}