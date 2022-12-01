using System.Collections.Generic;

namespace ObjectPrinting.Solved.Tests
{
    public class PersonWithFamily
    {
        public Person Person { get; set; }
        public IDictionary<string, Person> Family { get; set; }

        public PersonWithFamily(Person p)
        {
            Person = p;
            Family = new Dictionary<string, Person>();
        }

        public void AddFamilyMember(string degreeOfConsanguinity,Person p) => Family.Add(degreeOfConsanguinity,p);
    }
}