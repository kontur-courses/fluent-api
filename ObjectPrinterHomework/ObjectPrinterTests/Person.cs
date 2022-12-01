using System;

namespace ObjectPrinterTests
{
    public class Person
    {
        public Person? mother;
        public Person? father;
        public Guid Id { get; set; }
        public string Name;
        public string Surname;
        public double Height { get; set; }
        public int Age { get; set; }
        public Person(Person? mother, Person? father, Guid id, string name, string surname, double height, int age)
        {
            this.mother = mother;
            this.father = father;
            Id = id;
            Name = name;
            Height = height;
            Age = age;
            Surname = surname;
        }
    }
}