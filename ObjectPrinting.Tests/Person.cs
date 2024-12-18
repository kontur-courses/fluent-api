using System;
using System.Runtime.InteropServices;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public string LastName;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public Person Husband { get; set; }
        public Person Wife { get; set; }
        public List<string> HomeworksCompleted { get; set; }
        public Person[] Friends { get; set; }
        public Dictionary<string, string> EmergensyList { get; set; }
    }
}