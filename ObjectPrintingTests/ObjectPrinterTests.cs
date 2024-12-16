using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

[TestFixture, NonParallelizable]
public class ObjectPrinterTests
{
    private Person person;
    private PrintingConfig<Person> printer;

    [SetUp]
    public void SetUp()
    {
        printer = ObjectPrinter.For<Person>();
        person = new Person()
        {
            Name = "Johnny",
            Surname = "Cage",
            HouseAddress = new HouseAddress()
            {
                City = "Kansas",
                HouseNumber = "21A"
            },
            Age = 30,
            Height = 182.6,
            IsMarried = false,
            BirthDate = new DateTime(2003, 8, 12)
        };
    }

    [Test]
    public void PrintToString_ReturnCorrect_WhenExcludedType()
    {
        var expected = new[] { "false", "true", "IsMarried" };

        var actual = printer
            .Exclude<bool>()
            .PrintToString(person);

        actual.Should().NotContainAll(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WhenExcludedProperty()
    {
        var expected = "IsMarried";

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
        var expected = "Height = 182,60";

        var actual = printer
            .Printing<double>()
            .Using(x => x.ToString("F"))
            .PrintToString(person);

        actual.Should().Contain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WithCultureForFormattableTypes()
    {
        var expected = "Height = 182.6";

        var actual = printer
            .Printing<double>()
            .Using(CultureInfo.InvariantCulture)
            .PrintToString(person);

        actual.Should().Contain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WithCustomPropertySerialization()
    {
        var expected = "182,60 cm";

        var actual = printer
            .Printing(prop => prop.Height)
            .Using(x => $"{x:F} cm")
            .PrintToString(person);

        actual.Should().Contain(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrect_WithTrimForStrings()
    {
        var expected = new string[] { "Name = Jo", "Surname = Ca" };

        var actual = printer
            .Printing<string>()
            .Trimed(2)
            .PrintToString(person);

        actual.Should().ContainAll(expected);
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
        var expected = new [] {"Parents = Person[]\r\n\t\tPerson"};
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

    [Test]
    public void PrintToString_ExcludeStringsIncludeCustomSerialization()
    {
        var expected = new[] { "Name", "Surname" };

        var actual = printer
            .Exclude<string>()
            .Printing<string>()
            .Using(x => x.ToUpper())
            .PrintToString(person);

        actual.Should().NotContainAll(expected);
    }

    [Test]
    public void PrintToString_IncludeCustomSerializationExcludeStrings()
    {
        var expected = new[] { "Name", "Surname" };

        var actual = printer
            .Printing<string>()
            .Using(x => x.ToUpper())
            .Exclude<string>()
            .PrintToString(person);

        actual.Should().NotContainAll(expected);
    }

    [Test]
    public void PrintToString_ExcludeDictionary()
    {
        var expected = "Documents";

        var actual = printer
            .Exclude<Dictionary<string, string>>()
            .PrintToString(person);

        actual.Should().NotContain(expected);
    }

    [Test]
    public void PrintToString_ExcludeCollection()
    {
        var expected = "Parents";

        var actual = printer
            .Exclude<Person[]>()
            .PrintToString(person);

        actual.Should().NotContain(expected);
        actual.Should().Contain("BestFriend");
    }

    [Test]
    public void PrintToString_DateTimeSerialization()
    {
        var expected = "BirthDate = 12.08.03";

        var actual = printer
            .Printing<DateTime>()
            .Using(x => x.ToString("dd.MM.yy"))
            .PrintToString(person);

        actual.Should().Contain(expected);
    }

    [Test]
    public void PrintToString_ExcludeMultipleTypes()
    {
        var expected = new[] { "IsMarried", "Age" };

        var actual = printer
            .Exclude<int>()
            .Exclude<bool>()
            .PrintToString(person);

        actual.Should().NotContainAll(expected);
    }

    [Test]
    public void PrintToString_CombinedCustomSerializationAndExclusion()
    {
        var expected = "Height = 182,60";

        var actual = printer
            .Printing<double>()
            .Using(x => x.ToString("F"))
            .Exclude<bool>()
            .PrintToString(person);

        actual.Should().Contain(expected);
        actual.Should().NotContain("IsMarried");
    }

    [Test]
    public void PrintToString_MultipleExclusionsForSameType()
    {
        var expected = "Age";

        var actual = printer
            .Exclude<int>()
            .Exclude<int>()
            .PrintToString(person);

        actual.Should().NotContain(expected);
        printer.GetExcludeTypes().Should().HaveCount(1);
    }

    [Test]
    public void PrintToString_AcceptanceTest()
    {
        var expected = new[] { 
            "Name = JOHNNY", 
            "Surname = Ca",
            "Height = 182.60",
            "BestFriend",
            "BirthDate = 08/12/2003",  
            "Documents",
            "City = Ka",
            "HouseNumber = 21"
        };

        var actual = printer
            .Exclude<bool>()
            .Exclude<Person[]>()
            .Exclude(x => x.Age)
            .Printing(x => x.Name)
            .Using(x => x.ToUpper())
            .Printing<DateTime>()
            .Using(CultureInfo.InvariantCulture)
            .Printing<string>()
            .Trimed(2)
            .Printing<double>()
            .Using(x => x.ToString("F", CultureInfo.InvariantCulture))
            .PrintToString(person);

        actual.Should().ContainAll(expected);
    }
}