using System.Globalization;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

[TestFixture]
public class ObjectPrinter_Should
{
    private ObjectPrinter printer;
    private static readonly VerifySettings Settings = new();

    [SetUp]
    public void SetUp()
    {
        printer = new ObjectPrinter();
        Settings.UseDirectory("TestResults");
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenSerialize()
    {
        var person = new SimplePerson { Name = "Dima", Age = 20 };
        var printedString = printer.Serialize(person);
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenExcludeProperty()
    {
        var person = new SimplePerson { Name = "Dima", Age = 20 };
        var printedString = printer.Serialize(person,
            c => c.Exclude(x => x.Age));
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenExcludeType()
    {
        var person = new SimplePerson { Name = "Dima", Age = 20 };
        var printedString = printer.Serialize(person,
            c => c.Exclude<int>());
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenCustomPropertySerialization()
    {
        var person = new SimplePerson { Name = "Dima", Age = 20 };
        var printedString = printer.Serialize(person,
            c => c.SetCustomSerialization(x => x.Name, _ => "Name"));
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenCustomTypeSerialization()
    {
        var person = new SimplePerson { Name = "Dima", Age = 20 };
        var printedString = printer.Serialize(person,
            c => c.SetCustomSerialization<int>(_ => "Age"));
        return Verify(printedString, Settings);
    }

    [TestCase("ru-RU", ",")]
    [TestCase("en-US", ".")]
    public Task ObjectPrinter_ShouldHandle_WhenSetCultureForNumber(string culture, string expectedSeparator)
    {
        var person = new SimplePerson { Name = "Dima", Age = 20, Height = 180.5 };
        var printedString = printer.Serialize(person,
            c => c.SetCulture<double>(new CultureInfo(culture)));
        return Verify(printedString, Settings);
    }

    [TestCase("ru-RU")]
    [TestCase("en-US")]
    public Task ObjectPrinter_ShouldFormatDateTime_WhenSetCultureForDate(string culture)
    {
        var person = new DateTime(2024,12,11);
        var printedString = printer.Serialize(person,
            c => c.SetCulture<DateTime>(new CultureInfo(culture), "G"));
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenTrimString()
    {
        var person = new SimplePerson { Name = "Dima", Age = 20 };
        var printedString = printer.Serialize(person,
            c => c.TrimStringsToLength(2));
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenLengthIsGreaterThanTrimmed()
    {
        var person = new SimplePerson { Name = "Dima", Age = 20 };
        var printedString = printer.Serialize(person,
            c => c.TrimStringsToLength(10));
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenLargeCollectionWithTrim()
    {
        var largeCollection = Enumerable.Range(1, 1000).ToList();
        var printedString = printer.Serialize(largeCollection,
            c => c.TrimStringsToLength(50));
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenCircularReference()
    {
        var person = new PersonWithRoots { Name = "Dima", Age = 20 };
        person.Root = person;
        var printedString = printer.Serialize(person);
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenArray()
    {
        var first = new SimplePerson { Name = "Dima", Age = 20 };
        var second = new SimplePerson { Name = "Andrey", Height = 190, Age = 45 };

        SimplePerson[] persons = [first, second];
        var printedString = printer.Serialize(persons);
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenList()
    {
        List<SimplePerson> personList =
        [
            new() { Name = "Dima", Age = 20 },
            new() { Name = "Andrey", Age = 45 },
            new() { Name = "Vadim", Age = 10 }
        ];

        var printedString = printer.Serialize(personList);
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenNestedArray()
    {
        SimplePerson[] personArray =
        [
            new() { Name = "Dima", Age = 20 },
            new() { Name = "Andrey", Age = 45 },
            new() { Name = "Vadim", Age = 10 }
        ];

        var printedString = printer.Serialize(personArray);
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenDictionary()
    {
        Dictionary<SimplePerson, string> personDictionary = new()
        {
            { new SimplePerson { Name = "Dima", Age = 20 }, "first"},
            { new SimplePerson { Name = "Andrey", Age = 45 }, "second"},
            { new SimplePerson { Name = "Vadim", Age = 10 }, "third"}
        };

        var printedString = printer.Serialize(personDictionary);
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenCustomCollectionSerialization()
    {
        var collection = new List<int> { 1, 2, 3 };
        var printedString = printer.Serialize(collection,
            c => c.SetCustomSerialization<List<int>>(list => string.Join(", ", list)));
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenDeepNesting()
    {
        var nested = new NestedObject
        {
            Name = "Root",
            Child = new NestedObject
            {
                Name = "Child",
                Child = new NestedObject { Name = "GrandChild" }
            }
        };

        var printedString = printer.Serialize(nested);
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenFaultyItemInCollection()
    {
        var collection = new List<object>
        {
            new SimplePerson { Name = "Dima", Age = 20 },
            new FaultyType(),
            new SimplePerson { Name = "Andrey", Age = 45 }
        };

        var printedString = printer.Serialize(collection);
        return Verify(printedString, Settings);
    }

    [Test]
    public Task ObjectPrinter_ShouldHandle_WhenExcessiveNesting()
    {
        var deeplyNested = new NestedObject { Name = "Level 0" };
        var current = deeplyNested;
        for (var i = 1; i <= 100; i++)
        {
            current.Child = new NestedObject { Name = $"Level {i}" };
            current = current.Child;
        }

        var printedString = printer.Serialize(deeplyNested);
        return Verify(printedString, Settings);
    }
}