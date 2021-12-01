using System.Collections.Generic;

namespace ObjectPrinting.Tests.TestClasses
{
    public class ObjectWithNestedDictionary
    {
        public Dictionary<string, Dictionary<string, int>> Dictionary { get; set; }
    }
}