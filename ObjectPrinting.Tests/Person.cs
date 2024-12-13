using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public Person Husband { get; set; }
        public Person Wife { get; set; }
    }
}