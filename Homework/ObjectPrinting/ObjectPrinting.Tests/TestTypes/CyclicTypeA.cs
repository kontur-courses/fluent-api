using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Tests.TestTypes
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")] // "get" used implicitly
    public class CyclicTypeA
    {
        public CyclicTypeB CyclicProperty { get; set; }
    }
}