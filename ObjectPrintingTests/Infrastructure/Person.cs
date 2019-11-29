using System;

namespace ObjectPrintingTests.Infrastructure
{
    public class Person
    {
        public float Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        
        public Person[] Friends { get; set; }

        private int budget;

        public Person(float id, string name, double height, int age, Person[] friends)
        {
            Id = id;
            Name = name;
            Height = height;
            Age = age;
            Friends = friends;
        }
    }
}