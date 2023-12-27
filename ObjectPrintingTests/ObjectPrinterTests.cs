using System.Globalization;

namespace ObjectPrintingTests;

public class ObjectPrinterTests
{
    private Person person;
    private string newLine;

    [SetUp]
    public void Setup()
    {
        person = new Person { Name = "Alex", Age = 19, Height = 186.5 };
        newLine = Environment.NewLine;
    }

    [Test]
    public void PrintingConfig_ExcludeMemberType_ShouldExcludeGivenType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>();

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
    }

    [Test]
    public void PrintingConfig_ExcludeProperty_ShouldExcludeGivenProperty()
    {
        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Name);

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Name)} = {person.Name}");
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
    public void PrintingConfig_SetPrintingForType_ShouldUseCustomMethod()
    {
        var typePrinting = "int";

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor<int>().Using(p => typePrinting);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Age)} = {typePrinting}");
    }

    [Test]
    public void PrintingConfig_SetPrintingForProperty_ShouldUseCustomMethod()
    {
        var propertyPrinting = "Id";

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor(p => p.Id).Using(prop => propertyPrinting);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Id)} = {propertyPrinting}");
    }

    [Test]
    public void PrintingConfig_SetPrintingCulture_ShouldUseGivenCulture()
    {
        var culture = CultureInfo.CreateSpecificCulture("fr-FR");

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor<double>().WithCulture(culture);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Age)} = {person.Age.ToString(culture)}");
    }

    [Test]
    public void PrintingConfig_SetPrintingTrim_ShouldReturnTrimmedValue()
    {
        person.Name = "Alexxxx";
        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor(p => p.Name).TrimmedToLength(4);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Name)} = {person.Name[..4]}");
    }

    [Test]
    public void PrintingConfig_SetStringTypeTrim_ShouldReturnTrimmedString()
    {
        person.Name = "Alexxxx";
        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor<string>().TrimmedToLength(4);

        var result = printer.PrintToString(person);

        result.Should().Contain($"{nameof(person.Name)} = {person.Name[..4]}");
    }

    [Test]
    public void PrintingConfig_PrintCycledObject_ShouldDetectCycleReference()
    {
        person.Friend = person;

        var result = person.PrintToString();

        result.Should().Contain($"{nameof(person.Friend)} = Cycle reference detected");
    }

    [Test]
    public void PrintToString_PrintObject_ShouldPrintAllPublicMembers()
    {
        var result = person.PrintToString();

        foreach (var property in person.GetType().GetProperties())
        {
            result.Should().Contain($"{property.Name}");
        }

        foreach (var field in person.GetType().GetFields())
        {
            result.Should().Contain($"{field.Name}");
        }
    }

    [Test]
    public void PrintingConfig_PrintNestedObject_ShouldPrintNestedObject()
    {
        person.Friend = new Person { Name = "John", Age = 15, Height = 178.4 };

        var result = person.PrintToString();

        result.Should().Contain($"\t\t{nameof(person.Friend.Name)} = {person.Friend.Name}");
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

            result.Should().Contain($"\t\t{i}: {nameof(Person)}{newLine}");
            result.Should().Contain($"\t\t\t{nameof(friend.Id)} = {friend.Id}{newLine}");
            result.Should().Contain($"\t\t\t{nameof(friend.Name)} = {friend.Name}");
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

            result.Should().Contain($"\t\t{i}: {nameof(Person)}{newLine}");
            result.Should().Contain($"\t\t\t{nameof(relative.Id)} = {relative.Id}{newLine}");
            result.Should().Contain($"\t\t\t{nameof(relative.Name)} = {relative.Name}");
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

            result.Should().Contain($"\t\t{key}: {nameof(Person)}{newLine}");
            result.Should().Contain($"\t\t\t{nameof(neighbour.Id)} = {neighbour.Id}{newLine}");
            result.Should().Contain($"\t\t\t{nameof(neighbour.Name)} = {neighbour.Name}");
        }
    }

    struct Structure
    {
        public int intValue => 1;
        public string stringValue => "value";
    }

    [Test]
    public void PrintToString_PrintStructure_ShouldSerializeStructure()
    {
        var struc = new Structure();
        var result = struc.PrintToString();

        result.Should().Contain($"{nameof(struc.intValue)} = {struc.intValue}");
        result.Should().Contain($"{nameof(struc.stringValue)} = {struc.stringValue}");
    }
}