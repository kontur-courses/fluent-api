using System;
using System.Diagnostics.Contracts;
using System.Net.Mail;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Height { get; set; }
        public int Age { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
    }
}