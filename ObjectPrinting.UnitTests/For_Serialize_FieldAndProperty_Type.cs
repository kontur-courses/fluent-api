namespace ObjectPrinting.UnitTests;

public class For_Serialize_FieldAndProperty_Type
{
    public string Serialize = TestingConstants.TestStringValue;

    public string Test = TestingConstants.TestStringValue;
    public string SerializeProperty { get; set; } = TestingConstants.TestStringValue;
}