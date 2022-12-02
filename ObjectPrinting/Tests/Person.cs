using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }

        public Car Car { get; set; }

        public List<string> VisitedCountries { get; set; }

        public string[] Citizenships { get; set; }

        public Dictionary<int, int> IncomeTaxByYear { get; set; }

        public List<Person> Children { get; set; }

        public Dictionary<string, Person> Parents { get; set; }
    }
}