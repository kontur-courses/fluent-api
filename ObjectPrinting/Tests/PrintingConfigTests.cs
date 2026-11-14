using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class PrintingConfigTests
{
    private static readonly Person Alex = new ()
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Name = "Alex",
        Height = 180,
        Age = 19
    };

    [Test]
    public void PrintToString_ShouldPrintAllProperties_WhenNothingIsConfigured_Test()
    {
        var printer = ObjectPrinter.For<Person>();
        var expected = string.Join(
            Environment.NewLine,
            "Person",
            "\tId = 11111111-1111-1111-1111-111111111111",
            "\tName = Alex",
            "\tHeight = 180",
            "\tAge = 19");

        var printed = printer.PrintToString(Alex);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldSkipProperties_WhenTheirTypeIsExcludedByGenericArgument_Test()
    {
        var printer = ObjectPrinter
            .For<Person>()
            .Exclude<Guid>();
        var expected = string.Join(
            Environment.NewLine,
            "Person",
            "\tName = Alex",
            "\tHeight = 180",
            "\tAge = 19");

        var printed = printer.PrintToString(Alex);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldSkipProperties_WhenTheirTypeIsExcludedByTypeArgument_Test()
    {
        var printer = ObjectPrinter
            .For<Person>()
            .Exclude(typeof(double));
        var expected = string.Join(
            Environment.NewLine,
            "Person",
            "\tId = 11111111-1111-1111-1111-111111111111",
            "\tName = Alex",
            "\tAge = 19");

        var printed = printer.PrintToString(Alex);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldSkipPropertiesOfEachType_WhenTypesAreExcludedByCollection_Test()
    {
        var printer = ObjectPrinter
            .For<Person>()
            .Exclude([typeof(Guid), typeof(double), typeof(int)]);
        var expected = string.Join(
            Environment.NewLine,
            "Person",
            "\tName = Alex");

        var printed = printer.PrintToString(Alex);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldUseCustomPrinter_WhenItIsSetForType_Test()
    {
        var printer = ObjectPrinter
            .For<Person>()
            .Printing<int>()
            .Using(age => age + " years");
        var expected = string.Join(
            Environment.NewLine,
            "Person",
            "	Id = 11111111-1111-1111-1111-111111111111",
            "	Name = Alex",
            "	Height = 180",
            "	Age = 19 years");

        var printed = printer.PrintToString(Alex);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldUseLastPrinter_WhenSeveralAreSetForSameType_Test()
    {
        var printer = ObjectPrinter
            .For<Person>()
            .Printing<string>()
            .Using(_ => "first")
            .Printing<string>()
            .Using(name => name.ToUpper());
        var expected = string.Join(
            Environment.NewLine,
            "Person",
            "	Id = 11111111-1111-1111-1111-111111111111",
            "	Name = ALEX",
            "	Height = 180",
            "	Age = 19");

        var printed = printer.PrintToString(Alex);

        printed
            .Should()
            .Be(expected);
    }

    [TestCase("ru-RU", "1,85")]
    [TestCase("en-US", "1.85")]
    public void PrintToString_ShouldUseCultureOnlyForConfiguredType_WhenCultureIsSet_Test(
        string cultureName,
        string expectedHeight)
    {
        var person = new Person { Name = "Alex", Height = 1.85, Age = 19 };
        var printer = ObjectPrinter
            .For<Person>()
            .Exclude<Guid>()
            .Printing<double>()
            .Using(new CultureInfo(cultureName))
            .Printing<int>()
            .Using("D3", new CultureInfo("ru-RU"));
        var expected = string.Join(
            Environment.NewLine,
            "Person",
            "	Name = Alex",
            "	Height = " + expectedHeight,
            "	Age = 019");

        var printed = printer.PrintToString(person);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldUseFormat_WhenItIsSetForType_Test()
    {
        var printer = ObjectPrinter
            .For<Person>()
            .Printing<int>()
            .Using("X");
        var expected = string.Join(
            Environment.NewLine,
            "Person",
            "	Id = 11111111-1111-1111-1111-111111111111",
            "	Name = Alex",
            "	Height = 180",
            "	Age = 13");

        var printed = printer.PrintToString(Alex);

        printed
            .Should()
            .Be(expected);
    }
}