using System;

namespace ObjectPrinting.Tests.TestClasses
{
    public class PersonWithParent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public PersonWithParent Parent { get; set; }
        public string Surname { get; set; }
    }
}