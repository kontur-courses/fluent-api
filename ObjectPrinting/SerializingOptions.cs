namespace ObjectPrinting;

public struct SerializingOptions
{
    public int NestingLevel { get; init; }
    public string ElementOffset { get; init; }
    public string PreviousIndent { get; init; }
}