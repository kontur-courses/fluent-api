using System;

namespace ObjectPrinting.Tests
{
    public class Person2
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Person2 Friend { get; set; }
    }
}