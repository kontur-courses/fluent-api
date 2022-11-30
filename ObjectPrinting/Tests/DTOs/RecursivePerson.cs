using System;

namespace ObjectPrinting.Tests.DTOs
{
    public class RecursivePerson
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public RecursivePerson Father { get; set; }
    }
}