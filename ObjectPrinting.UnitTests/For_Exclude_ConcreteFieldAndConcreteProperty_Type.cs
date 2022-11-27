namespace ObjectPrinting.UnitTests;

public class For_Exclude_ConcreteFieldAndConcreteProperty_Type
{
    public string Field = TestingConstants.TestStringValue;
    public string Property { get; set; } = TestingConstants.TestStringValue;

    public string Test { get; set; } = TestingConstants.TestStringValue;
}