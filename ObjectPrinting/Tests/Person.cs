using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public bool IsMale { get; set; }
        public Car Car { get; set; }
        //public List<Person> Children { get; set; }
        //public Person Parent { get; set; }

        public Person(Guid id, string name, double height, int age, bool isMale, Car car)
        {
            Id = id;
            Name = name;
            Height = height;
            Age = age;
            IsMale = isMale;
            Car = car;
        }
    }
}