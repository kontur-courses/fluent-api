using System;

namespace ObjectPrintingTests.TestingSource
{
    public class Person : ICloneable
    {
        public Guid Id { get; set; }
        public string Name { get; init; }
        public double Height { get; init; }
        public int Age { get; init; }

        public object Clone()
        {
            return new Person()
            {
                Id = Guid.NewGuid(),
                Name = this.Name,
                Height = this.Height,
                Age = this.Age
            };
        }
    }
}