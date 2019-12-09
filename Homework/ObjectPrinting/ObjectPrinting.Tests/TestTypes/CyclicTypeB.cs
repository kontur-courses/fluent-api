using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Tests.TestTypes
{
    // "get" used implicitly when calling PrintToString() for instance in PrintToStringTests class.
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CyclicTypeB
    {
        public CyclicTypeA CyclicProperty { get; set; }
    }
}