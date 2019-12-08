using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Tests.TestTypes
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")] // "get" used implicitly
    public class CyclicTypeB
    {
        public CyclicTypeA CyclicProperty { get; set; }
    }
}