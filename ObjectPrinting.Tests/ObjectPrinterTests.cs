using System;
using System.Collections.Generic;
using NUnit.Framework;
using AwesomeAssertions;
using System.Globalization;

namespace ObjectPrinting.Tests;

public class ObjectPrinterTests
{
    private Person person = null!;
    // используется фиксированное значение для verify
    private readonly Guid fixedGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly string SnapshotsDirectory = "Snapshots";
    
    [SetUp]
    public void SetUp()
    {
        person = new Person { Age = 20, Height = 185.5, Id = fixedGuid, Name = "yey" };
    }
    
    private static SettingsTask VerifySnapshot(object? value)
    {
        var settings = new VerifySettings();
        settings.UseDirectory(SnapshotsDirectory);

        return Verify(value, settings);
    }

    [Test]
    public Task PrintingConfig_PrintsPersonData_WhenNoFilters()
    {
        var printer = ObjectPrinter.For<Person>();
        var actual = printer.PrintToString(person);

        return VerifySnapshot(actual);
    }

    [Test]
    public Task PrintingConfig_DoesNotPrintProperty_WhenPropertyExcluded()
    {
        var printer = ObjectPrinter.For<Person>();
        printer.Exclude(p => p.Name);
        
        var actual = printer.PrintToString(person);
        
        return VerifySnapshot(actual);
    }

    [Test]
    public Task PrintingConfig_DoesNotPrintProperty_WhenTypeExcluded()
    {
        var printer = ObjectPrinter.For<Person>();
        printer.Exclude<string>();
        
        var actual = printer.PrintToString(person);
        
        return VerifySnapshot(actual);
    }

    [Test]
    public Task PrintingConfig_CustomSerializing_WhenSetModifiedSerialization()
    {
        var printer = ObjectPrinter.For<Person>();
        printer.SetModifiedSerialization(p => p.Id,
            guid => $"{guid} - Globally Unique Identifier");

        var actual = printer.PrintToString(person);

        return VerifySnapshot(actual);
    }

    [Test]
    public Task PrintingConfig_CustomSerializing_WhenSetTypeSerialization()
    {
        var printer = ObjectPrinter.For<Person>();
        printer.SetTypeSerialization<Guid>(guid => $"{guid} - Globally Unique Identifier");

        var actual = printer.PrintToString(person);

        return VerifySnapshot(actual);
    }

    [Test]
    public Task PrintingConfig_UsesSpecifiedCulture_WhenNumericCultureSet()
    {
        var culture = CultureInfo.GetCultureInfo("fr-FR");

        var printer = ObjectPrinter.For<Person>();
        printer.SetNumericCulture(culture);

        var actual = printer.PrintToString(person);
        return VerifySnapshot(actual);
    }

    [Test]
    public Task PrintingConfig_TruncatesString_WhenTruncateSet()
    {
        var printer = ObjectPrinter.For<Person>();
        printer.Truncate(p => p.Name, 2);

        var actual = printer.PrintToString(person);
        return VerifySnapshot(actual);
    }

    [Test]
    public void PrintingConfig_DoesNotTruncate_WhenLengthExceedsStringLength()
    {
        var printer = ObjectPrinter.For<Person>();
        printer.Truncate(p => p.Name, 100);

        var actual = printer.PrintToString(person);

        actual.Should().Contain("\tName = yey" + Environment.NewLine);
    }

    [Test]
    public void PrintingConfig_Throws_WhenTruncateLengthIsNegative()
    {
        var printer = ObjectPrinter.For<Person>();

        var action = () => printer.Truncate(p => p.Name, -1);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void PrintingConfig_HandlesCyclicReferences()
    {
        var node = new Node();
        node.Next = node;

        var printer = ObjectPrinter.For<Node>();

        var actual = printer.PrintToString(node);

        var expected = string.Join(Environment.NewLine,
            "Node",
            "\tNext = Node - cycle link",
            ""
        );

        actual.Should().Be(expected);
    }

    [Test]
    public void PrintingConfig_PrintsArray_WhenArrayProvided()
    {
        var numbers = new[] { 10, 20, 30 };

        var expected = string.Join(Environment.NewLine,
            "Int32[]",
            "\t[0] = 10",
            "\t[1] = 20",
            "\t[2] = 30",
            ""
        );

        var printer = ObjectPrinter.For<int[]>();

        var actual = printer.PrintToString(numbers);
        actual.Should().Be(expected);
    }

    [Test]
    public void PrintingConfig_PrintsList_WhenStringListProvided()
    {
        var names = new List<string> { "aa", "bb", "cc" };

        var expected = string.Join(Environment.NewLine,
            "List`1",
            "\t[0] = aa",
            "\t[1] = bb",
            "\t[2] = cc",
            ""
        );

        var printer = ObjectPrinter.For<List<string>>();

        var actual = printer.PrintToString(names);
        actual.Should().Be(expected);
    }

    [Test]
    public Task PrintingConfig_PrintsListOfObjects_WhenComplexTypeListProvided()
    {
        var people = new List<Person> { person };
        var printer = ObjectPrinter.For<List<Person>>();

        var actual = printer.PrintToString(people);
        
        return VerifySnapshot(actual);
    }

    [Test]
    public void PrintingConfig_PrintsDictionary_WhenDictionaryProvided()
    {
        var dictionary = new Dictionary<string, int>
        {
            ["aa"] = 20,
            ["bb"] = 25
        };

        var expected = string.Join(Environment.NewLine,
            "Dictionary`2",
            "\t[aa] = 20",
            "\t[bb] = 25",
            ""
        );

        var printer = ObjectPrinter.For<Dictionary<string, int>>();

        var actual = printer.PrintToString(dictionary);
        actual.Should().Be(expected);
    }

    [Test]
    public void PrintingConfig_UsesCustomTypeSerializer_WhenListProvided()
    {
        var numbers = new List<int> { 10, 20, 30 };

        var expected = string.Join(Environment.NewLine,
            "List`1",
            "\t[0] = Number: 10",
            "\t[1] = Number: 20",
            "\t[2] = Number: 30",
            ""
        );

        var printer = ObjectPrinter.For<List<int>>();
        printer.SetTypeSerialization<int>(number => $"Number: {number}");

        var actual = printer.PrintToString(numbers);
        actual.Should().Be(expected);
    }

    [Test]
    public void PrintingConfig_PrintsPublicField_WhenFieldProvided()
    {
        var obj = new FieldSample { Number = 42 };

        var expected = string.Join(Environment.NewLine,
            "FieldSample",
            "\tNumber = 42",
            ""
        );

        var printer = ObjectPrinter.For<FieldSample>();

        var actual = printer.PrintToString(obj);
        actual.Should().Be(expected);
    }

    [Test]
    public void PrintingConfig_DoesNotPrintField_WhenFieldExcluded()
    {
        var obj = new FieldSample { Number = 42 };

        var expected = string.Join(Environment.NewLine,
            "FieldSample",
            ""
        );

        var printer = ObjectPrinter.For<FieldSample>();
        printer.Exclude(x => x.Number);

        var actual = printer.PrintToString(obj);
        actual.Should().Be(expected);
    }
    
    [Test]
    public void PrintingConfig_DoesNotPrintPrivateField_WhenObjectProvided()
    {
        var obj = new FieldSample { Number = 42 };

        var expected = string.Join(Environment.NewLine,
            "FieldSample",
            "\tNumber = 42",
            ""
        );

        var printer = ObjectPrinter.For<FieldSample>();

        var actual = printer.PrintToString(obj);
        actual.Should().Be(expected);
    }

    [Test]
    public void PrintingConfig_DoesNotDetectCycle_WhenReferenceReused()
    {
        var node = new Node();

        var obj = new ReferenceHolder
        {
            First = node,
            Second = node
        };

        var printer = ObjectPrinter.For<ReferenceHolder>();

        var actual = printer.PrintToString(obj);

        actual.Should().Contain("\tFirst = Node" + Environment.NewLine);
        actual.Should().Contain("\tSecond = Node" + Environment.NewLine);
        actual.Should().NotContain("cycle link");
    }

    [Test]
    public void PrintingConfig_HandlesCyclicReferences_WhenCollectionContainsItself()
    {
        var items = new List<object>();
        items.Add(items);

        var expected = string.Join(Environment.NewLine,
            "List`1",
            "\t[0] = List`1 - cycle link",
            ""
        );

        var printer = ObjectPrinter.For<List<object>>();

        var actual = printer.PrintToString(items);
        actual.Should().Be(expected);
    }

    [Test]
    public void PrintingConfig_PrintsNull_WhenCollectionContainsNull()
    {
        var items = new List<string?> { "aa", null, "bb" };

        var expected = string.Join(Environment.NewLine,
            "List`1",
            "\t[0] = aa",
            "\t[1] = null",
            "\t[2] = bb",
            ""
        );

        var printer = ObjectPrinter.For<List<string?>>();

        var actual = printer.PrintToString(items);
        actual.Should().Be(expected);
    }

    [Test]
    public void PrintingConfig_PrintsNestedCollections_WhenListContainsLists()
    {
        var numbers = new List<List<int>>
        {
            new() { 10, 20 },
            new() { 30 }
        };

        var expected = string.Join(Environment.NewLine,
            "List`1",
            "\t[0] = List`1",
            "\t\t[0] = 10",
            "\t\t[1] = 20",
            "\t[1] = List`1",
            "\t\t[0] = 30",
            ""
        );

        var printer = ObjectPrinter.For<List<List<int>>>();

        var actual = printer.PrintToString(numbers);
        actual.Should().Be(expected);
    }

    [Test]
    public void PrintingConfig_PrioritizesMemberSerializer_WhenBothSerializersSet()
    {
        var printer = ObjectPrinter.For<Person>();

        printer.SetTypeSerialization<string>(value => $"Type: {value}");
        printer.SetModifiedSerialization(p => p.Name, value => $"Member: {value}");

        var actual = printer.PrintToString(person);

        actual.Should().Contain($"\tName = Member: {person.Name}" + Environment.NewLine);
        actual.Should().NotContain($"\tName = Type: {person.Name}");
    }

    [Test]
    public void PrintingConfig_UsesCustomTypeSerializer_WhenRootObjectProvided()
    {
        var printer = ObjectPrinter.For<int>();
        printer.SetTypeSerialization<int>(number => $"Number: {number}");

        var expected = "Number: 42" + Environment.NewLine;

        var actual = printer.PrintToString(42);
        actual.Should().Be(expected);
    }
}