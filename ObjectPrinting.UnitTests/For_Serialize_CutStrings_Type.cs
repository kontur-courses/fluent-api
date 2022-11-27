namespace ObjectPrinting.UnitTests;

public class For_Serialize_CutStrings_Type
{
    public string CutUpTo25 = TestingConstants.LongTestStringValue;
    public string CutUpTo50 { get; set; } = TestingConstants.LongTestStringValue;

    public string CutUpTo75Other { get; set; } = TestingConstants.LongTestStringValue;
}