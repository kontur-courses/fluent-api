using System;

namespace ObjectPrinting.Tests
{
    public class Person3
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Person3 Friend1 { get; set; }
        public Person3 Friend2 { get; set; }
    }
}