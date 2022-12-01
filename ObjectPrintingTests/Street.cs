using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting.Tests
{
    internal class Street
    {
        public Guid Id { get; set; }
        public List<House> Houses { get; set; }
        public Dictionary<Person, House> AddressBook { get; set; }
        public Person[] HonorablePersonsOfStreet { get; set; }
    }
}
