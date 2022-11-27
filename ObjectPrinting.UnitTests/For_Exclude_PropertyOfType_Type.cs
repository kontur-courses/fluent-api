namespace ObjectPrinting.UnitTests;

public class For_Exclude_PropertyOfType_Type
{
    public TestingVoid Exclude { get; set; } = new();

    public string Test { get; } = TestingConstants.TestStringValue;
}