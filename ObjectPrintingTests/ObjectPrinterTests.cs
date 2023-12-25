using System.Globalization;

namespace ObjectPrintingTests;

public class ObjectPrinterTests
{
    private Person person;

    [SetUp]
    public void Setup()
    {
        person = new Person { Name = "Alex", Age = 19, Height = 186.5 };
    }

    [Test]
    public void PrintingConfig_ExcludePropertyType_ShouldExcludeGivenType()
    {
        var printer = ObjectPrinter.For<Person>()
            .ExcludePropertyType<Guid>();

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
    }

    [Test]
    public void PrintingConfig_ExcludeProperty_ShouldExcludeGivenProperty()
    {
        var printer = ObjectPrinter.For<Person>()
            .ExcludeProperty(p => p.Name);

        var result = printer.PrintToString(person);

        result.Should().NotContain($"{nameof(person.Name)} = {person.Name}");
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
        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor(p => p.Name).TrimmedToLength(4);

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
    public void PrintingConfig_PrintNestedObject_ShouldPrintNestedObject()
    {
        person.Friend = new Person { Name = "John", Age = 15, Height = 178.4 };

        var result = person.PrintToString();

        result.Should().Contain($"\t\t{nameof(person.Friend.Name)} = {person.Friend.Name}");
    }
}