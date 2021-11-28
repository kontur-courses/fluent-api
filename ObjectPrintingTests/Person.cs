using System;

namespace ObjectPrintingTests
{
    public class Person
    {
        public int Age;
        public DateTime Birthdate;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public Person Father { get; set; }
        public Person Mother { get; set; }
    }
}