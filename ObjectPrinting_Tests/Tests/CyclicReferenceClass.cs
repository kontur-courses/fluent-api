namespace ObjectPrinting.Tests
{
    public class CyclicReferenceClass
    {
        public CyclicReferenceClass AnotherClass { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
    }
}