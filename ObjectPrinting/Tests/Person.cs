using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public string Surname;
        public Person Friend { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Person)) return false;
            var otherPerson = (Person) obj;
            return Id.Equals(otherPerson.Id);
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode() * 997) ^ Name.GetHashCode();
        }
    }
}