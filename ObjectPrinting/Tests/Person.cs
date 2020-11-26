using System;
using System.Collections;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public DateTime Birthday { get; set; }
        public Pet Dog { get; set; }
        public Person Friend { get; set; }
        public string Nickname;
        public ICollection PhoneNumbers;
    }
}