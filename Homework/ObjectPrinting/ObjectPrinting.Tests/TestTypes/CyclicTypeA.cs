using System.Diagnostics.CodeAnalysis;

namespace ObjectPrinting.Tests.TestTypes
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class CyclicTypeA
    {
        public CyclicTypeB CyclicProperty { get; set; }
    }
}