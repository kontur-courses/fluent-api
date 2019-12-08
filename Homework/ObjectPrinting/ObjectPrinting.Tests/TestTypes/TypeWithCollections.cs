using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Tests.TestTypes
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")] // "get" used implicitly
    public class TypeWithCollections
    {
        public int[] Numbers { get; set; }
        public List<string> Strings { get; set; }
        public Dictionary<string, int> NumberByString { get; set; }
    }
}