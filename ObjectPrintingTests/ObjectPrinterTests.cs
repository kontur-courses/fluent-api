using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.Configs;
using ObjectPrinting.Configs.Extensions;
using ObjectPrintingTests.Models;

namespace ObjectPrintingTests;

public class ObjectPrinterTests
{
    private static string LoadExpected() =>
        File.ReadAllText(
            $"{TestContext.CurrentContext.TestDirectory}/../../../ExpectedTestsResults/{TestContext.CurrentContext.Test.MethodName}.txt");

    [Test]
    public void ObjectPrinter_ShouldPrintNull_WhenObjectIsNull_Test()
    {
        var printer = ObjectPrinter.InClass<Person>();
        var result = printer.PrintToString(null);
        result.Should().Be("null");
    }

    [Test]
    public void ObjectPrinter_ShouldPrintFullProperty_WhenSerializerReturnsNull_Test()
    {
        var person = new Person { Name = "Alex" };

        var printer = ObjectPrinter.InClass<Person>()
            .For(p => p.Name).Use(_ => null);

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = Alex");
    }

    [Test]
    public void ObjectPrinter_ShouldPrintSimpleObject_WhenPropertiesArePrimitive_Test()
    {
        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Alex",
            Height = 180.5,
            Age = 25,
            Friend = null,
            Scores = null
        };

        var printer = ObjectPrinter.InClass<Person>();
        var result = printer.PrintToString(person);

        var expected = LoadExpected();
        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldExcludeType_WhenTypeIsExcluded_Test()
    {
        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Alex",
            Height = 181,
            Age = 30,
            Friend = null,
            Scores = null
        };

        var printer = ObjectPrinter.InClass<Person>().For<Guid>().Exclude();
        var result = printer.PrintToString(person);

        var expected = LoadExpected();
        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldExcludeProperty_WhenPropertyIsExcluded_Test()
    {
        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Alex",
            Height = 181,
            Age = 30,
            Friend = null,
            Scores = null
        };

        var printer = ObjectPrinter.InClass<Person>().For(p => p.Age).Exclude();
        var result = printer.PrintToString(person);

        var expected = LoadExpected();
        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldUseCustomTypeSerializer_WhenSerializerForTypeProvided_Test()
    {
        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Alex",
            Height = 170,
            Age = 18,
            Friend = null,
            Scores = null
        };

        var printer = ObjectPrinter.InClass<Person>()
            .For<int>().Use(i => $"Int:{i}");

        var result = printer.PrintToString(person);
        var expected = LoadExpected();

        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldUseCustomPropertySerializer_WhenSerializerForPropertyProvided_Test()
    {
        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Alex",
            Height = 170,
            Age = 18,
            Friend = null,
            Scores = null
        };

        var printer = ObjectPrinter.InClass<Person>()
            .For(p => p.Name).Use(v => $"NAME({v})");

        var result = printer.PrintToString(person);
        var expected = LoadExpected();

        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldApplyCulture_WhenCultureIsSpecifiedForType_Test()
    {
        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Alex",
            Height = 1234.56,
            Age = 10,
            Friend = null,
            Scores = null
        };

        var printer = ObjectPrinter.InClass<Person>()
            .For<double>().Use(new CultureInfo("de-DE"));

        var result = printer.PrintToString(person);
        var expected = LoadExpected();

        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldTrimStrings_WhenTrimIsApplied_Test()
    {
        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Alexander",
            Height = 100,
            Age = 5,
            Friend = null,
            Scores = null
        };

        var printer = ObjectPrinter.InClass<Person>()
            .For(p => p.Name).Trim(4);

        var result = printer.PrintToString(person);
        var expected = LoadExpected();

        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldPrintList_WhenListContainsPrimitives_Test()
    {
        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Test",
            Height = 100,
            Age = 10,
            Friend = null,
            Scores = new List<int> { 1, 2, 3 }
        };

        var printer = ObjectPrinter.InClass<Person>();
        var result = printer.PrintToString(person);
        var expected = LoadExpected();

        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldPrintNestedObject_WhenObjectContainsInnerObject_Test()
    {
        var friend = new Person
        {
            Id = Guid.Empty,
            Name = "John",
            Height = 150,
            Age = 20
        };

        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Alex",
            Height = 180,
            Age = 30,
            Friend = friend
        };

        var printer = ObjectPrinter.InClass<Person>();
        var result = printer.PrintToString(person);

        var expected = LoadExpected();
        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldHandleCyclicReferences_WhenCycleDetected_Test()
    {
        var a = new Person { Name = "A" };
        var b = new Person { Name = "B" };
        a.Friend = b;
        b.Friend = a;

        var printer = ObjectPrinter.InClass<Person>();
        var result = printer.PrintToString(a);

        var expected = LoadExpected();
        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldIncreaseIndentation_WhenObjectIsNested_Test()
    {
        var person = new Person
        {
            Id = Guid.Empty,
            Name = "Alex",
            Height = 180,
            Age = 10,
            Friend = new Person
            {
                Id = Guid.Empty,
                Name = "John",
                Height = 150,
                Age = 20
            }
        };

        var printer = ObjectPrinter.InClass<Person>();
        var result = printer.PrintToString(person);

        var expected = LoadExpected();
        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldPrintEmptyObject_WhenAllPropertiesDefault_Test()
    {
        var person = new Person();
        var printer = ObjectPrinter.InClass<Person>();

        var result = printer.PrintToString(person);
        var expected = LoadExpected();

        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldPrintDictionary_WhenDictionaryContainsPrimitives_Test()
    {
        var dict = new Dictionary<string, int>
        {
            ["one"] = 1,
            ["two"] = 2
        };

        var printer = ObjectPrinter.InClass<Dictionary<string, int>>();
        var result = printer.PrintToString(dict);

        var expected = LoadExpected();
        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldPrintNestedCollections_WhenCollectionsAreNested_Test()
    {
        var data = new List<List<int>>
        {
            new() { 1, 2 },
            new() { 3, 4 }
        };

        var printer = ObjectPrinter.InClass<List<List<int>>>();
        var result = printer.PrintToString(data);

        var expected = LoadExpected();
        result.Should().Be(expected);
    }

    [Test]
    public void ObjectPrinter_ShouldFormatDateTime_WithCustomCulture_Test()
    {
        var date = new DateTime(2025, 5, 1, 13, 45, 0);

        var printer = ObjectPrinter.InClass<DateTime>()
            .For<DateTime>().Use(new CultureInfo("de-DE"));

        var result = printer.PrintToString(date);

        result.Should().Be("01.05.2025 13:45:00");
    }

    [Test]
    public void ObjectPrinter_ShouldApplyTrimBeforeSerializer_Test()
    {
        var person = new Person { Name = "Alexander" };

        var printer = ObjectPrinter
            .InClass<Person>()
            .For(p => p.Name).Trim(4)
            .For(p => p.Name).Use(s => $"<{s}>");


        var result = printer.PrintToString(person);

        result.Should().Contain("<Alex>");
    }

    [Test]
    public void ObjectPrinter_ShouldApplyCultureThenTypeSerializer_Test()
    {
        var person = new Person { Height = 1234.567 };

        var printer = ObjectPrinter.InClass<Person>()
            .For<double>().Use(new CultureInfo("fr-FR"))
            .For<double>().Use(d => $"<{d}>");

        var result = printer.PrintToString(person);

        result.Should().Contain("<1234,567>");
    }
}