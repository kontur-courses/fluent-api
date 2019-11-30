using System;
using System.Collections.Generic;

namespace ObjectPrinterTests
{
    public class Person
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Surname { get; }

        public double Height { get; }
        public int Age { get; }

        public Person(Guid id, string name, string surname, double height, int age)
        {
            Id = id;
            Name = name;
            Surname = surname;
            Height = height;
            Age = age;
        }

        public Dictionary<string, string> GetDefaultPersonSerializingDictionary()
        {
            return new Dictionary<string, string>
            {
                {nameof(Id), "Guid"},
                {nameof(Name), Name},
                {nameof(Surname), Surname},
                {nameof(Height), Height.ToString()},
                {nameof(Age), Age.ToString()}
            };
        }
    }
}