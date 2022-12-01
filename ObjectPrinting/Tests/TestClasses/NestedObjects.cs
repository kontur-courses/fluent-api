using System;

namespace ObjectPrinting.Tests
{
    public class NestedObjectOne
    {
        public NestedObjectTwo Ref { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

    public class NestedObjectTwo
    {
        public string Surname { get; set; }
        public DateTime Birthday { get; set; }
    }
}