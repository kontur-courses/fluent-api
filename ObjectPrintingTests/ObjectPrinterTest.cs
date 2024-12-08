using System.Globalization;
using FluentAssertions;
using ObjectPrinting;

namespace ObjectPrintingTests;

[TestFixture]
[TestOf(typeof(ObjectPrinter))]
public class ObjectPrinterTest
{
    [Test]
    public void PrintingConfig_Exclude_ShouldExcludeGivenType()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>();
        var s1 = printer.PrintToString(person);

        s1.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
    }

    [Test]
    public void PrintingConfig_Exclude_ShouldExcludeGivenProperty()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Name);

        var s1 = printer.PrintToString(person);

        s1.Should().NotContain($"{nameof(person.Name)} = {person.Name}");
    }

    [Test]
    public void PrintingConfig_SetPrintingForType_ShouldUseCustomMethod()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        const string intValue = "int";

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor<int>()
            .Using(_ => intValue);

        var s1 = printer.PrintToString(person);

        s1.Should().Contain($"{nameof(person.Age)} = {intValue}");
    }

    [Test]
    public void PrintingConfig_SetPrintingForProperty_ShouldUseCustomMethod()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        const string idValue = "Id";

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor(p => p.Id)
            .Using(_ => idValue);

        var s1 = printer.PrintToString(person);

        s1.Should().Contain($"{nameof(person.Id)} = {idValue}");
    }
    
    [Test]
    public void PrintingConfig_SetPrintingCulture_ShouldUseGivenCulture()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 189.25 };
        var culture = CultureInfo.CreateSpecificCulture("fr-FR");

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor<double>().WithCulture(culture);

        var s1 = printer.PrintToString(person);

        s1.Should().Contain($"{nameof(person.Age)} = {person.Age.ToString(culture)}");
    }

    [Test]
    public void PrintingConfig_SetPrintingTrim_ShouldReturnTrimmedValue()
    {
        var person = new Person { Name = "Alexxx", Age = 19, Height = 189.25 };

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor(p => p.Name).TrimmedToLength(4);

        var s1 = printer.PrintToString(person);

        s1.Should().Contain($"{nameof(person.Name)} = Alex");
    }

    [Test]
    public void PrintingConfig_PrintCycledObject_ShouldDetectCycleReference()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        person.Friend = person;

        var s1 = person.PrintToString();

        s1.Should().Contain($"{nameof(person.Friend)} = Cycle reference detected");
    }
}