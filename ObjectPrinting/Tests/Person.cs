using System;
using System.Collections.Generic;

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

        public int[] SomeArray { get; set; } = {1, 2, 3, 5, 7};

        public Dictionary<string, int> SomeDict { get; set; } = new Dictionary<string, int>()
        {
            ["a"] = 1,
            ["b"] = 2,
            ["c"] = 7
        };

        public int Age
        {
            get => (DateTime.Now - BirthDate).Days / 365;
            set => BirthDate = DateTime.Now - TimeSpan.FromDays(value * 365);
        }
    }
}