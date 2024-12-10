namespace ObjectPrinting.Tests;

public class Recursion
{
    public RecursionObject Object { get; set; }
}

public record class RecursionObject
{
    public RecursionObject Object { get; set; }
}
