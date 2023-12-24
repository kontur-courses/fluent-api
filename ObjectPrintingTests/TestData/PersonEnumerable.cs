namespace ObjectPrintingTests.TestData;

public class PersonEnumerable
{
    public int Field = 2;
    public TimeSpan Property { get; set; }
    public List<PersonEnumerable> List { get; set; } = new();
}