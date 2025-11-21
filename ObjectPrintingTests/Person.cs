using System;

namespace ObjectPrinting.Solved.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public decimal Money { get; set; }
        public Person Parent { get; set; }
        public DateTime Birthday { get; set; }
        public List<string> FriendsNames { get; set; }
        public DateTime[] FriendsBirthdays { get; set; }
        public Dictionary<string, string> Pets { get; set; }
    }
}