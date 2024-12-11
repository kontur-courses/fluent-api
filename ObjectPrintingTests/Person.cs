using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string NameOfPet { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        
        public int[] CountsOfTeamMembers { get; set; }
        
        public List<string> AlliedTeams { get; set; }

        public Dictionary<Person, string> Team { get; set; }
    }
}