using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Height { get; set; }
        protected int secretNumber = 1;
        private int SecretNumber => secretNumber;
        public int Age { get; set; }
        public Person BestFriend { get; set; }
        public DateTime Birthday => DateTime.Now;
    }
}