using System;

namespace ObjectPrintingTests.Persons
{
    public class FieldPerson
    {
        public Guid Id;
        public string Name;
        public double Height;
        public int Age;

        public FieldPerson(Guid id, string name, double height, int age)
        {
            Id = id;
            Name = name;
            Height = height;
            Age = age;
        }
    }
}