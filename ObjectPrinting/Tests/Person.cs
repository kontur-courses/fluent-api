using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Person Mother { get; set; }
        public Person Father { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public DateTime BirthDate { get; set; }

        public int Age
        {
            get => (DateTime.Now - BirthDate).Days / 365;
            set => BirthDate = DateTime.Now - TimeSpan.FromDays(value * 365);
        }
    }
}