namespace ObjectPrintingTests;

public class FaultyType
{
    public override string ToString() => throw new Exception("Serialization error!");
}