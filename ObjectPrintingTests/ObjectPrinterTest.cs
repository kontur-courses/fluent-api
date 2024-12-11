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
        person = new Person(new Guid(), "Alex",189.25, 19);
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
    public void PrintingConfig_PrintCycledObject_ShouldNotFail()
    {
        person.Friend = person;

        var result = person.PrintToString();

        result.Should().Contain("Достигнут максимум глубины сериализации");
    }
    
    [Test]
    public void PrintingConfig_PrintNestedObject_ShouldPrintNestedObject()
    {
        person.Friend = new Person();
    
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
        var friends = new List<Person>{new(), new()};
    
        person.Friends = friends;
    
        var result = person.PrintToString();
    
        for (var i = 0; i < friends.Count; i++)
        {
            var friend = friends[i];
            
            result.Should().Contain($"\t\t{i}: {nameof(Person)}{newLine}");
            result.Should().Contain($"\t\t\t{nameof(friend.Id)} = {friend.Id}{newLine}");
            result.Should().Contain($"\t\t\t{nameof(friend.Name)} = {friend.Name}");
        }
    }
    
    [Test]
    public void PrintToString_PrintClassWithArray_ShouldSerializeArray()
    {
        var relatives = new[]
            { new Person(), new Person()};
    
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
        var neighbours = new Dictionary<int, Person>
            { { 12, new Person() }, { 19, new Person() } };
    
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