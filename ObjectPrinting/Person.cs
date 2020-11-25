using System;

namespace ObjectPrinting
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        private string Password { get; set; }
        public Person OtherPerson { get; set; }
    }
}