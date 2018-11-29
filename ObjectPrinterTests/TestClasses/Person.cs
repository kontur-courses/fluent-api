using System;

namespace ObjectPrinterTests.TestClasses
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SecondName;
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime Birthday { get; set; }
        public TimeSpan WorkExperience { get; set; }

        public Person Child { get; set; }

        public int GetAge()
        {
            return Age;
        }

        public Person(string guid)
        {
            Name = "Andrey";
            Age = 21;
            Height = 1.92;
            Id = Guid.Parse(guid);
            Birthday = new DateTime(2018, 01, 28);
            SecondName = "Ivanov";
            WorkExperience = TimeSpan.FromDays(3);
        }
    }
}