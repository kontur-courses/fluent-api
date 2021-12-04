using System;
using System.Collections.Generic;
using AutoFixture;

namespace ObjectPrintingTaskTests.TestData
{
    public class Person
    {
        public Person Parent { get; set; }
        public double Weight;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public List<Person> Family { get; set; } = new List<Person>();

        public static Person GetTestInstance()
        {
            return new Person
            {
                Id = Guid.NewGuid(),
                Name = "Marsell",
                Surname = "Radkevich",
                Age = new Fixture().Create<int>(),
                Height = new Fixture().Create<double>() - 0.5,
                Weight = new Fixture().Create<double>() - 0.5
            };
        }       
    }
}