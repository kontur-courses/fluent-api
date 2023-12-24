namespace ObjectPrintingTests.TestData;

public class EnumerableTest
{
    public readonly double[] Array = {1.2, 2.12, 3.2, 4.0};

    public readonly List<ComplexPerson> List;

    public Dictionary<int, DateTime> Dictionary;

    public EnumerableTest(List<ComplexPerson> list, Dictionary<int, DateTime> dictionary)
    {
        List = list;
        Dictionary = dictionary;
    }
}