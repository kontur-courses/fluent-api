namespace ObjectPrintingTests;

public class CyclicReference
{
    public string Name { get; set; }
    public CyclicReference Obj { get; set; }
}