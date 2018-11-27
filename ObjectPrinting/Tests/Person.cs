using System;
using System.Runtime.InteropServices;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Pet Pet { get; set; }

        public Person(string name, double height, int age)
        {
            Name = name;
            Height = height;
            Age = age;
        }

        public Person() { }
    }

    public class Pet
    {
        public string Name { set; get; }
        public int Age { get; set; }

        public Pet(string name)
        {
            Name = name;
        }
    }
}