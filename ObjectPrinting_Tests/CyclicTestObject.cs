namespace ObjectPrinting_Tests;

public class CyclicTestObject
{
    public string Str { get; set; }
    public CyclicTestObject TestObject { get; set; }
}