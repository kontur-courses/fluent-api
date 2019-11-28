using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class PersonWithCollection
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public List<string> EmailList { get; set; }
        public List<string> PhoneNumbers { get; set; }
    }
}