namespace ObjectPrinting.UnitTests;

public class For_Serialize_FieldOrPropertyThatImplementsIFormattable_Type
{
    public float Field = TestingConstants.TestFloatValue;

    public double Property { get; set; } = TestingConstants.TestDoubleValue;
}