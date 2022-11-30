using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public Person Father { get; set; }
        public List<Person> Children { get; set; }
        public int[] Sizes { get; set; }
        public Dictionary<string, Person> Relatives { get; set; }
        public Dictionary<string, int> Companies { get; set; }
    }
}