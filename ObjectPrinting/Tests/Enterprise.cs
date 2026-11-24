using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Tests
{
    public class Enterprise
    {
        public string Name { get; set; }
        public DateTime FoundedDate { get; set; }
        public Director Director { get; set; }
        public List<Employee> Employees { get; set; } = new();
        public Dictionary<string, double> Assets { get; set; } = new();
        public string Description { get; set; }
        public Enterprise ParentCompany { get; set; }
    }
}
