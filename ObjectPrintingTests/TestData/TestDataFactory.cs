namespace ObjectPrintingTests.TestData;

public static class TestDataFactory
{
    public static readonly Person Person = new(new Guid(), "Alex", 192.2, 33);

    public static readonly ComplexPerson ComplexPerson = new(new Guid(), "Ivan", 190.1, 30);

    public static readonly ComplexPerson Parent = new(new Guid(), "Asd", 0.12, 12);

    public static readonly EnumerableTest EnumerableSimpleTest = new(new List<ComplexPerson> {ComplexPerson},
        new Dictionary<int, DateTime> {{1, new DateTime(1, 1, 1)}, {3, new DateTime(1988, 2, 28)}});

    public static readonly EnumerableTest
        EnumerableEmptyTest = new(new List<ComplexPerson>(), new Dictionary<int, DateTime>());

    public static readonly ComplexEnumerable ComplexEnumerable = new();
}