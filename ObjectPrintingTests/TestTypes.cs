using System;
using System.Collections.Generic;

namespace ObjectPrintingTests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public string Citizenship { get; set; }
    }

    public class PersonWithParent
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public PersonWithParent Parent { get; set; }
    }

    public class PersonWithParentContainer
    {
        public PersonWithParent Person1 { get; set; }
        public PersonWithParent Person2 { get; set; }
    }

    public class Class
    {
        public List<Person> Students { get; set; }
        public int ClassNumber { get; set; }
    }
}