namespace ObjectPrintingTests;

public class CycledObject
{
    public CycledObject Cycle { get; }

    public CycledObject()
    {
        Cycle = this;
    }
}