namespace ObjectPrinting_Tests;

public class TestObject
{
    public int Int1 { get; }
    public int Int2 { get; }
    public string Str { get; }
    public double Double { get; }

    public TestObject(int i1, int i2, string str, double d)
    {
        Int1 = i1;
        Int2 = i2;
        Str = str;
        Double = d;
    }
}