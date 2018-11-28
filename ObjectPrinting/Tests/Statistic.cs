using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Statistic
    {
        public string Name { get; set; }
        public Dictionary<string, int> Logbook { get; set; }
        public List<Person> Persons;
        public Person GroupLeader;

        public Statistic(string name, Person groupLeader)
        {
            Name = name;
            GroupLeader = groupLeader;
            Persons = new List<Person>();
            Logbook = new Dictionary<string, int>();
        }

    }
}