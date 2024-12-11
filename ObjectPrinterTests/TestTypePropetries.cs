namespace ObjectPrinterTests;

public class TestTypePropetries
{
    public int Int { get; set; }
    public double Double { get; set; }
    public DateTime DateTime { get; set; }
    public float Float { get; set; }
    
    

    public static TestTypePropetries GetTestType()
    {
        return new TestTypePropetries()
        {
            Int = 1,
            DateTime = new DateTime(2002, 1, 11),
            Double = 1.1,
            Float = 134.45E-2f
        };
    }
}