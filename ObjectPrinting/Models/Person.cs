using System;
using System.Collections.Generic;

namespace ObjectPrinting.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime Birthday { get; set; }
        public int Weight;
        public Person Parent { get; set; }
        public string[] Aliases { get; set; }
        public Person[] Childrens { get; set; }
        public List<int> FavoriteNumbers { get; set; }
        public Dictionary<string, DateTime> Tasks { get; set; }
        public Location Location { get; set; }
    }
}