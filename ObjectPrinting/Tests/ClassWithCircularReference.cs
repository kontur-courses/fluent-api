namespace ObjectPrinting.Tests
{
    public class ClassWithCircularReference
    {
        public string ObjectName { get; set; }
        public ClassWithCircularReference OtherObject { get; set; }
    }
}
