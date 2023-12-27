using System.Collections.Generic;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class Collections
    {
        public Dictionary<int, string> Dictionary { set; get; }
        public List<int>[] Array { set; get; }
        public List<int> List { set; get; }
        
        public List<Person> Persons { set; get; }
    }
}