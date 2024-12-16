using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

[Parallelizable(ParallelScope.All)]
public class PrintingConfigShould
{
    private static readonly VerifySettings Settings = new();

    [SetUp]
    public void SetUp()
    {
        Settings.UseDirectory("PrintingResults");
    }

    [Test]
    public void PrintToString_ShouldNotThrowStackOverflowException_WhenRecursion()
    {
        var alex = new Person(Guid.Empty, "Alex", 188, 111, DateTime.MinValue, BestField: 11);
        var alexWithParent = GetPersonWithRecursiveParent(alex);

        var action = () => ObjectPrinter.For<Person>().PrintToString(alexWithParent);

        action.Should().NotThrow<StackOverflowException>();
    }

    private static Person GetPersonWithRecursiveParent(Person person)
    {
        var newPerson = person with { Parent = person };
        for (var i = 0; i < 10; i++)
        {
            newPerson = newPerson with { Parent = newPerson };
        }

        return person with { Parent = newPerson };
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task PrintToString_ShouldHasShortString_AfterCutTextLength(int maxLength)
    {
        var alex = new Person(Guid.Empty, "Alex", 188, 111, DateTime.MinValue, BestField: 11);

        var fullText = ObjectPrinter.For<Person>()
            .Printing(p => p.Name)
            .TrimmedToLength(maxLength)
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrint_WithSetSerializationForProperty()
    {
        var alex = new Person(Guid.Empty, "Alex", 188, 111, DateTime.MinValue, BestField: 11);

        var fullText = ObjectPrinter.For<Person>()
            .Printing(p => p.Height)
            .Using(h => $"{h} meters")
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }

    [TestCase("X")]
    [TestCase("C")]
    [TestCase("F")]
    public Task PrintToString_ShouldPrint_WithChangedSerialization(string format)
    {
        var alex = new Person(Guid.Empty, "Alex", 188, 111, DateTime.MinValue, BestField: 11);

        var fullText = ObjectPrinter.For<Person>()
            .Printing<int>()
            .Using(i => i.ToString(format))
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrint_WithSetCulture()
    {
        var alex = new Person(Guid.Empty, "Alex", 188, 111, DateTime.MinValue, BestField: 11);

        var fullText = ObjectPrinter.For<Person>()
            .Printing<DateTime>()
            .Using(CultureInfo.InvariantCulture)
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrintArrayOfPerson_WithoutChanges()
    {
        var personArray = new[]
        {
            new Person(new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), "Alex", 188, 111, DateTime.MinValue,
                BestField: 11),
            new Person(new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), "Alex1", 111, 11, DateTime.MinValue,
                BestField: 11),
            new Person(new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae1"), "Alex2", 183, 111, DateTime.MaxValue,
                BestField: 11)
        };

        var fullText = ObjectPrinter
            .For<Person[]>()
            .PrintToString(personArray);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrintDictionaryOfPerson_WithoutChanges()
    {
        var personArray = new[]
        {
            new Person(new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), "Alex", 188, 111, DateTime.MinValue,
                BestField: 11),
            new Person(new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), "Alex1", 111, 11, DateTime.MinValue,
                BestField: 11),
            new Person(new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae1"), "Alex2", 183, 111, DateTime.MaxValue,
                BestField: 11)
        };
        var personDictionary = personArray.ToDictionary(person => person.Id, person => person);

        var fullText = ObjectPrinter
            .For<Dictionary<Guid, Person>>()
            .PrintToString(personDictionary);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrint_WithoutExcludedPropertyOrField()
    {
        var alex = new Person(Guid.Empty, "Alex", 188, 111, DateTime.MinValue, BestField: 11);

        var fullText = ObjectPrinter.For<Person>()
            .Excluding(p => p.Age)
            .Excluding(p => p.BestField)
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrint_WithoutExcludedPropertyOrFieldByType()
    {
        var alex = new Person(Guid.Empty, "Alex", 188, 111, DateTime.MinValue, BestField: 11);

        var fullText = ObjectPrinter.For<Person>()
            .Excluding<int>()
            .Excluding<decimal>()
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }
}