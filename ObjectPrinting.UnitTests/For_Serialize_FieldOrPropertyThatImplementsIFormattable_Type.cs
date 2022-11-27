namespace ObjectPrinting.UnitTests;

public class For_Serialize_FieldOrPropertyThatImplementsIFormattable_Type
{
    public double Field = TestingConstants.TestDoubleValue;

    public double Property { get; set; } = TestingConstants.TestDoubleValue;
}