using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Tests.TestTypes
{
    // "get" used implicitly when calling PrintToString() for instance in PrintToStringTests class.
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class TypeWithCollections
    {
        public int[] Numbers { get; set; }
        public List<string> Strings { get; set; }
        public Dictionary<string, int> NumberByString { get; set; }
    }
}