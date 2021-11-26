using System;

namespace ObjectPrinting.Solved.TestData
{
    public class Person
    {
        public double SomeField;

        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }

        public static Person GetInstance()
        {
            return new Person
            {
                Id = Guid.NewGuid(),
                Name = "Marsell",
                Height = 195,
                Age = 21,
                SomeField = 2.0
            };
        }
    }
}