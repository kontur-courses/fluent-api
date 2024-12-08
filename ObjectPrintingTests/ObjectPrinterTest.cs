using System.Globalization;
using FluentAssertions;
using ObjectPrinting;

namespace ObjectPrintingTests;

[TestFixture]
[TestOf(typeof(ObjectPrinter))]
public class ObjectPrinterTest
{
    private Person person;

    [SetUp]
    public void SetUp()
    {
        person = new Person { Name = "Alex", Age = 19, Height = 189.25 };
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
}