namespace ObjectPrintingTests.TestData;

public static class TestDataFactory
{
    public static readonly Person Person = new(new Guid(), "Alex", 192.2, 33);

    public static readonly ComplexPerson ComplexPerson = new(new Guid(), "Ivan", 190.1, 30);

    public static readonly ComplexPerson Parent = new(new Guid(), "Asd", 0.12, 12);
}