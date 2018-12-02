using System;

namespace ObjectPrintingTests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }

        public Person Parent;
        public Person Child;
        public int[] LuckyNumbers;
        public int Weight;
        public float UselessField;
    }
}