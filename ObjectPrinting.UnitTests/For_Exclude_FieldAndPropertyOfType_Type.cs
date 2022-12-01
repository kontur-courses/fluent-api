namespace ObjectPrinting.UnitTests;

public class For_Exclude_FieldAndPropertyOfType_Type
{
    public TestingVoid Exclude = new();

    public TestingVoid Exclude2 { get; } = new();

    public string Test { get; } = TestingConstants.TestStringValue;
}