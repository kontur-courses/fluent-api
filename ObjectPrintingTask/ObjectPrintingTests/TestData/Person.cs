using System;

namespace ObjectPrintingTaskTests.TestData
{
    public class Person
    {
        public Person Parent { get; private set; }
        public double Weight;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }

        public void SetParent(Person parent)
        {
            Parent = parent;
        }

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