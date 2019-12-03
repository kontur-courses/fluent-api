using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Tests.TestTypes
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CyclicTypeB
    {
        public CyclicTypeA CyclicProperty { get; set; }
    }
}