namespace ObjectPrinting_Tests;

public class CyclicTestObject
{
    public int Number { get; set; }
    public CyclicTestObject Child1 { get; set; }
    public CyclicTestObject Child2 { get; set; }
    public CyclicTestObject Child3 { get; set; }
}