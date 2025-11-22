using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class Company
    {
        public string Name { get; set; }
        public List<Person> Employees { get; set; }
        public decimal Budget { get; set; }
    }
}
