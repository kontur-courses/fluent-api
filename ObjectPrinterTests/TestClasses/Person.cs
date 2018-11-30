using System;

namespace ObjectPrinterTests.TestClasses
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SecondName;
        public double Height { get; set; }
        public int Age1 { get; set; }
        public int Age2 { get; set; }
        public DateTime Birthday { get; set; }
        public TimeSpan WorkExperience { get; set; }

        public Person Child { get; set; }

        public int GetAge()
        {
            return Age1;
        }

        public Person(string guid)
        {
            Name = "Andrey";
            Age1 = 21;
            Age2 = 21;
            Height = 1.92;
            Id = Guid.Parse(guid);
            Birthday = new DateTime(2018, 01, 28);
            SecondName = "Ivanov";
            WorkExperience = TimeSpan.FromDays(3);
        }
    }
}