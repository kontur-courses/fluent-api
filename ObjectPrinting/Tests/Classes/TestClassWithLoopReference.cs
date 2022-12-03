namespace ObjectPrinting.Tests.Classes
{
    internal class TestClassWithLoopReference
    {
        public int Property { get; set; }
        public TestClassWithLoopReference LoopReference { get; set; }
    }
}