using System.Globalization;
using FluentAssertions;
using ObjectPrinting;

namespace ObjectPrintingTests;

[TestFixture]
[TestOf(typeof(ObjectPrinter))]
public class ObjectPrinterTest
{
    private Person person;
    private string newLine;

    [SetUp]
    public void SetUp()
    {
        person = new Person { Name = "Alex", Age = 19, Height = 189.25 };
        newLine = Environment.NewLine;
    }
    
    [Test]
    public void PrintingConfig_Exclude_ShouldExcludeGivenType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>();
        
        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
    }

    [Test]
    public void PrintingConfig_Exclude_ShouldExcludeGivenProperty()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Name);

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Name)} = {person.Name}");
    }

    [Test]
    public void PrintingConfig_SetPrintingForType_ShouldUseCustomMethod()
    {
        const string printingType = "int";

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor<int>()
            .Using(_ => printingType);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Age)} = {printingType}");
    }

    [Test]
    public void PrintingConfig_SetPrintingForProperty_ShouldUseCustomMethod()
    {
        const string printingProperty = "Id";

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor(p => p.Id)
            .Using(_ => printingProperty);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Id)} = {printingProperty}");
    }
    
    [Test]
    public void PrintingConfig_SetPrintingCulture_ShouldUseGivenCulture()
    {
        var culture = CultureInfo.CreateSpecificCulture("fr-FR");

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor<double>()
            .WithCulture(culture);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Age)} = {person.Age.ToString(culture)}");
    }

    [Test]
    public void PrintingConfig_SetPrintingTrim_ShouldReturnTrimmedValue()
    {
        const int trimLength = 4;
        
        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor(p => p.Name)
            .TrimmedToLength(trimLength);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Name)} = {person.Name[..trimLength]}");
    }

    [Test]
    public void PrintingConfig_PrintCycledObject_ShouldDetectCycleReference()
    {
        person.Friend = person;

        var result = person.PrintToString();

        result.Should().Contain($"{nameof(person.Friend)} = Cycle reference detected");
    }
    
    [Test]
    public void PrintingConfig_PrintNestedObject_ShouldPrintNestedObject()
    {
        person.Friend = new Person { Name = "John", Age = 15, Height = 178.4 };

        var result = person.PrintToString();

        result.Should().Contain($"\t\t{nameof(person.Friend.Name)} = {person.Friend.Name}");
    }
    
    [Test]
    public void PrintingConfig_ExcludeField_ShouldExcludeGivenProperty()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Field);

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Field)} = {person.Field}");
    }
    
    [Test]
    public void PrintToString_PrintClassWithList_ShouldSerializeList()
    {
        var friends = new List<Person>
            { new Person { Name = "Andy", Age = 21 }, new Person { Name = "Serj", Age = 17 } };

        person.Friends = friends;

        var result = person.PrintToString();

        for (var i = 0; i < friends.Count; i++)
        {
            var friend = friends[i];
            result.Should().Contain($"\t\t{i}: {nameof(Person)}{newLine}" +
                                    $"\t\t\t{nameof(friend.Id)} = {friend.Id}{newLine}" +
                                    $"\t\t\t{nameof(friend.Name)} = {friend.Name}");
        }
    }

    [Test]
    public void PrintToString_PrintClassWithArray_ShouldSerializeArray()
    {
        var relatives = new[]
            { new Person { Name = "Sarah", Age = 41 }, new Person { Name = "John", Age = 47 } };

        person.Relatives = relatives;

        var result = person.PrintToString();

        for (var i = 0; i < relatives.Length; i++)
        {
            var relative = relatives[i];
            result.Should().Contain($"\t\t{i}: {nameof(Person)}{newLine}" +
                                    $"\t\t\t{nameof(relative.Id)} = {relative.Id}{newLine}" +
                                    $"\t\t\t{nameof(relative.Name)} = {relative.Name}");
        }
    }

    [Test]
    public void PrintToString_PrintClassWithDictionary_ShouldSerializeDictionary()
    {
        var neighbours = new Dictionary<int, Person>()
            { { 12, new Person { Name = "Andy", Age = 21 } }, { 19, new Person { Name = "Serj", Age = 17 } } };

        person.Neighbours = neighbours;

        var result = person.PrintToString();

        foreach (var key in neighbours.Keys)
        {
            var neighbour = neighbours[key];
            result.Should().Contain($"\t\t{key}: {nameof(Person)}{newLine}" +
                                    $"\t\t\t{nameof(neighbour.Id)} = {neighbour.Id}{newLine}" +
                                    $"\t\t\t{nameof(neighbour.Name)} = {neighbour.Name}");
        }
    }
}