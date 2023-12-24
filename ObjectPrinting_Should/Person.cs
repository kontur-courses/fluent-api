using System;

namespace ObjectPrinting_Should
{
    public class Person
    {
        public Guid Id { get; set; }
        public Person Sibling { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public double[] FavouriteNumbers { get; set; }
    }
}