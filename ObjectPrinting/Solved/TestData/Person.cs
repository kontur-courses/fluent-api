using System;

namespace ObjectPrinting.Solved.TestData
{
    public class Person
    {
        public Person Parent;

        public double Weight;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }

        public static Person GetTestInstance()
        {
            return new Person
            {
                Id = Guid.NewGuid(),
                Name = "Marsell",
                Surname = "Radkevich",
                Age = 21,
                Height = 195.5,
                Weight = 83.4
            };
        }
    }
}