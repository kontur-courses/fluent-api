using System;

namespace ObjectPrinting
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public NonCulturable NonCulturable { get; set; }

        public double Weight;
        public string SecondName;
    }
}