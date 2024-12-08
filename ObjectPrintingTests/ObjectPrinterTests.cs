using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

[TestFixture, NonParallelizable] //поведение по умолчанию, но я решил явно указать
public class ObjectPrinterAcceptanceTests
{
    private Person person;
    private PrintingConfig<Person> printer;

    [SetUp]
    public void SetUp()
    {
        printer = ObjectPrinter.For<Person>();
        person = new Person()
        {
            Name = "John",
            HouseAddress = new HouseAddress()
            {
                City = "Kansas",
                HouseNumber = "21A"
            },
            Age = 30,
            Height = 182.6,
            IsMarried = false,
        };
    }

    [Test]
    public void PrintToString_ReturnCorrect_WhenExcludedType()
    {
        var expected = "false";

        var actual = printer
            .Exclude<bool>()
            .PrintToString(person);

        actual.Should().NotContain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WhenExcludedProperty()
    {
        var expected = "false";

        var actual = printer
            .Exclude(x => x.IsMarried)
            .PrintToString(person);

        actual.Should().NotContain(expected);
    }
    
    [Test]
    public void PrintToString_ReturnCorrect_WhenExcludedCustomType()
    {
        var expected = "HouseAddress";

        var actual = printer
            .Exclude<HouseAddress>()
            .PrintToString(person);

        actual.Should().NotContain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WithCustomTypeSerialization()
    {
        var expected = "Height = 182,6";

        var actual = printer
            .SerializeType<double>()
            .Using(x => x.ToString("F"))
            .PrintToString(person);

        actual.Should().Contain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WithCultureForFormattableTypes()
    {
        var expected = "Height = 182.6";

        var actual = printer
            .SerializeType<double>()
            .Using(CultureInfo.InvariantCulture)
            .PrintToString(person);

        actual.Should().Contain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WuthCustomPropertySerialization()
    {
        var expected = "182,60 cm";

        var actual = printer
            .SerializeProperty(prop => prop.Height)
            .Using(x => $"{x:F} cm")
            .PrintToString(person);

        actual.Should().Contain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WithTrimForStrings()
    {
        var expected = "Name = John";

        var actual = printer
            .SerializeType<string>()
            .Trimed(2)
            .PrintToString(person);

        actual.Should().NotContain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WithObjectExtensions()
    {
        var expected = printer
            .Exclude<bool>()
            .PrintToString(person);

        var actual = person
            .PrintToString(x => x.Exclude<bool>());

        actual.Should().Be(expected);
    }

    [Test]
    public void PrintToString_CatchReferenceCycles()
    {
        var expected ="Looped! Object was not added";

        person.Parents = new[] { person };
        var actual = person
            .PrintToString(x => x.Exclude<bool>());

        actual.Should().Contain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WhenPrintCollections()
    {
        var expected = new [] {"Parents = Person[]", "HouseAddress = null"};
        person.Parents = new[]
        {
            new Person {Height = 175, Age = 30},
            new Person {Height = 200, Age = 27},
            new Person {Height = 183, Age = 23},
            new Person {Height = 165, Age = 16},
        };

        var actual = person
            .PrintToString(x => x.Exclude<bool>());

        actual.Should().ContainAll(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WhenPrintDictionaries()
    {
        var expected = new[]
        {
            "Documents = Dictionary`2", 
            "Passport - 2215 124987", 
            "SNILS - 124 14245 9582 001"
        };
        person.Documents = new Dictionary<string, string>()
        {
            {"Passport", "2215 124987"},
            {"SNILS", "124 14245 9582 001"}
        };

        var actual = person
            .PrintToString(x => x.Exclude<bool>());

        actual.Should().ContainAll(expected);
    }
}