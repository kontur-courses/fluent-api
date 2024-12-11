namespace ObjectPrinting.Tests.TestObjects;

public class PrintingTestObject
{
    public Guid TestId { get; set; }
    public string TestString { get; set; }
    public double TestDouble { get; set; }
    public float TestFloat { get; set; }
    public int TestInt { get; set; }
    public object TestObject { get; set; }
    public Dictionary<string, object> TestDictionary { get; set; }
    public List<object> TestList { get; set; }
    public object[] TestArray { get; set; }
}