using System;
using System.Collections.Generic;

namespace ObjectPrintingTests.TestModels
{
    public class Office
    {
        public List<string> OfficeThings { get; set; }
        public DateTime[] Times { get; set; }
        public Dictionary<int, Person> Employees { get; set; }
    }
}