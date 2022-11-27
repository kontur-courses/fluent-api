namespace ObjectPrinting.UnitTests;

public class For_Exclude_FieldOfType_Type
{
    public TestingVoid Exclude = new();

    public string Test { get; } = TestingConstants.TestStringValue;
}