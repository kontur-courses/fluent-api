using System;
using System.Collections.Generic;

namespace ObjectPrinterTests.TestClasses
{
    public class Company
    {
        public string Name { get; set; }
        public DateTime EstablishedSince;

        public IEnumerable<Person> Employees;
    }
}
