namespace ObjectPrintingTests.TestData;

public class ComplexEnumerable
{
    public List<List<int>> List = new()
    {
        new List<int> {2},
        new List<int> {2, 3}
    };

    public readonly List<Person> Persons = new()
    {
        TestDataFactory.Person
    };

    public readonly Dictionary<string, Person> Dictionary = new()
    {
        {"asd", TestDataFactory.ComplexPerson}
    };
}