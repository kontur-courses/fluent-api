using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public int YearOfBirth { get; set; }
        public Person Parent { get; set; }
        public DateTime BirthDate { get; set; }
        public List<string> Friends { get; set; }
        public List<int> Scores { get; set; }
        public List<decimal> Prices { get; set; }
        public Dictionary<string, int> Grades { get; set; }
        public decimal Salary { get; set; }
        public string Description { get; set; }
        public List<List<int>> Matrix { get; set; }
        public Dictionary<string, Subject> SubjectDetails { get; set; }
        public int[] ScoresArray { get; set; }
    }
}