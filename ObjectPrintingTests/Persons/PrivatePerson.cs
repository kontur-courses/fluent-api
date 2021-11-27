using System;

namespace ObjectPrintingTests.Persons
{
    public class PrivatePerson
    {
        public Guid Id { get; set; }
        private string name = "Alex";
        private int age = 19;
    }
}