using System;

namespace ObjectPrinting.Tests.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Parent Father { get; set; }
        public Parent Mother { get; set; }
    }

    public class Parent : Person
    {

    }
}