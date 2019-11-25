namespace ObjectPrinting.Tests
{
    public class ClassWithCircularReference
    {
        public string ObjectName { get; set; }
        public ClassWithCircularReference NestedObject { get; set; }
    }
}
