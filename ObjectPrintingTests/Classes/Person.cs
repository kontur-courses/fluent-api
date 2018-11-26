using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public double Weight { get; set; }
        public int Field;
        public Person Parent { get; set; }
        public Person Relative { get; set; }

        public Person GetNextPerson()
        {
            return new Person() {Id = Id, Name = Name, Age = 10};
        }
    }
}