using System;

namespace ObjectPrinting.Tests.TestClasses
{
    public class Person
    {
        public string Company;
        public Person Child;
        public Car Car;
        public Person Mother => new Person();
        
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public float Width { get; set; }
        public int Age { get; set; }
        public float Mass;
    }
}