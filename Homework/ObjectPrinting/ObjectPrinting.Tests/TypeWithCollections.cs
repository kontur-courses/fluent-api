using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    public class TypeWithCollections
    {
        public int[] Numbers { get; set; }
        public List<string> Strings { get; set; }
        public Dictionary<string, int> NumberByString { get; set; }
    }
}