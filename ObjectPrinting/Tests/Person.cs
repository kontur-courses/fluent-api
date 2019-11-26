using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }

        public static Person CreatePerson()
        {
            return new Person {Name = "Alex", Age = 20, Height = 170.5};
        }
    }
}