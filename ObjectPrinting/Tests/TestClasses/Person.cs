using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Person Parent;

        private readonly float waklSpeed = 5;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public DateTime Birthday { get; set; }
        public float RunRatio { get; set; }
        public int Age { get; set; }
        public float RunSpeed => waklSpeed * RunRatio;
    }
}