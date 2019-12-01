namespace ObjectPrinting.Tests.TestTypes
{
    public class CyclicTypeA
    {
        public CyclicTypeB CyclicProperty { get; set; }
    }
}