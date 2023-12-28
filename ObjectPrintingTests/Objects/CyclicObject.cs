namespace ObjectPrintingTests.Objects;

public class CyclicObject
{
    public int SomeProp { get; set; }
    public CyclicObject Another { get; set; }
}