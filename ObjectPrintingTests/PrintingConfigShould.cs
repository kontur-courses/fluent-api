using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

[Parallelizable(ParallelScope.All)]
public class PrintingConfigShould
{
    private readonly PrintingConfig<Person> personPrintingConfig = ObjectPrinter.For<Person>();
    private readonly Person alex = new(Guid.Empty, "Alex", 188, 111, DateTime.MinValue, BestField: 11);

    private readonly Person[] personArray =
    [
        new(new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), "Alex", 188, 111, DateTime.MinValue, BestField: 11),
        new(new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), "Alex1", 111, 11, DateTime.MinValue, BestField: 11),
        new(new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae1"), "Alex2", 183, 111, DateTime.MaxValue, BestField: 11)
    ];

    private static readonly VerifySettings Settings = new();

    [SetUp]
    public void SetUp()
    {
        Settings.UseDirectory("PrintingResults");
    }

    [Test]
    public void PrintToString_ShouldNotThrowStackOverflowException_WhenRecursion()
    {
        var alexWithParent = GetPersonWithRecursiveParent(alex);

        var action = () => personPrintingConfig.PrintToString(alexWithParent);

        action.Should().NotThrow<StackOverflowException>();
    }

    private static Person GetPersonWithRecursiveParent(Person person) =>
        person with { Parent = person };

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public Task PrintToString_ShouldHasShortString_AfterCutTextLength(int maxLength)
    {
        var fullText = personPrintingConfig
            .Printing(p => p.Name)
            .TrimmedToLength(maxLength)
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrint_WithSetSerializationForProperty()
    {
        var fullText = personPrintingConfig
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
        var fullText = personPrintingConfig
            .Printing<int>()
            .Using(i => i.ToString(format))
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrint_WithSetCulture()
    {
        var fullText = personPrintingConfig
            .Printing<DateTime>()
            .Using(CultureInfo.InvariantCulture)
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrintArrayOfPerson_WithoutChanges()
    {
        var fullText = ObjectPrinter
            .For<Person[]>()
            .PrintToString(personArray);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrintDictionaryOfPerson_WithoutChanges()
    {
        var personDictionary = personArray.ToDictionary(person => person.Id, person => person);

        var fullText = ObjectPrinter
            .For<Dictionary<Guid, Person>>()
            .PrintToString(personDictionary);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrint_WithoutExcludedPropertyOrField()
    {
        var fullText = personPrintingConfig
            .Excluding(p => p.Age)
            .Excluding(p => p.BestField)
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }

    [Test]
    public Task PrintToString_ShouldPrint_WithoutExcludedPropertyOrFieldByType()
    {
        var fullText = personPrintingConfig
            .Excluding<int>()
            .Excluding<decimal>()
            .PrintToString(alex);

        return Verify(fullText, Settings);
    }
}