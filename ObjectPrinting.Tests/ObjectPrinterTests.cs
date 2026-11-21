using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterTests
{
    private class SimplePerson
    {
        public string Name { get; set; } = "John";
        public int Age { get; set; } = 30;
        public double Height { get; set; } = 180.5;
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    private class Node
    {
        public string Name { get; set; } = string.Empty;
        public Node? Next { get; set; }
    }

    private class CollectionHolder
    {
        public int[] Numbers { get; set; } = [];
        public List<string> Words { get; set; } = [];
        public Dictionary<string, int> Map { get; set; } = [];
    }

    [Test]
    public void PrintToString_NullObject_ReturnsNullString()
    {
        string? nullStr = null;

        var result = nullStr.PrintToString();

        result.Should().Be("null" + Environment.NewLine);
    }

    [Test]
    public void Exclude_Type_ExcludesAllPropertiesOfThatType()
    {
        var person = new SimplePerson();
        var printer = ObjectPrinter.Setup<SimplePerson>()
            .Exclude<int>()
            .Apply();

        var result = printer.PrintToString(person);

        result.Should().NotContain("Age =");
        result.Should().Contain("Name = John");
    }

    [Test]
    public void Exclude_SpecificProperty_ExcludesOnlySelectedProperty()
    {
        var person = new SimplePerson { Name = "Alice", Age = 25 };
        var printer = ObjectPrinter.Setup<SimplePerson>()
            .Exclude(p => p.Age)
            .Apply();

        var result = printer.PrintToString(person);

        result.Should().NotContain("Age =");
        result.Should().Contain("Name = Alice");
    }

    [Test]
    public void SetFormat_ForType_AppliesCustomFormatToAllPropertiesOfType()
    {
        var person = new SimplePerson { Age = 30 };
        var printer = ObjectPrinter.Setup<SimplePerson>()
            .For<int>()
                .SetFormat(age => $"{age} y.o.")
                .Then()
            .Apply();

        var result = printer.PrintToString(person);

        result.Should().Contain("Age = 30 y.o.");
    }

    [Test]
    public void SetFormat_ForSpecificProperty_AppliesCustomFormatOnlyToThatProperty()
    {
        var person = new SimplePerson { Name = "Bob" };
        var printer = ObjectPrinter.Setup<SimplePerson>()
            .For(p => p.Name)
                .SetFormat(name => $"MR. {name.ToUpperInvariant()}")
                .Then()
            .Apply();

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = MR. BOB");
    }

    [Test]
    public void SetCulture_ForType_AppliesCultureCorrectly()
    {
        var person = new SimplePerson { Height = 175.5 };
        var ruCulture = new CultureInfo("ru-RU");

        var printer = ObjectPrinter.Setup<SimplePerson>()
            .For<double>()
                .SetCulture(ruCulture)
                .Then()
            .Apply();

        var result = printer.PrintToString(person);

        result.Should().Contain("Height = 175,5");
    }

    [Test]
    public void Truncate_ForStringProperty_TruncatesStringCorrectly()
    {
        var person = new SimplePerson { Name = "Alexander" };
        var printer = ObjectPrinter.Setup<SimplePerson>()
            .For(p => p.Name)
                .Truncate(4)
                .Then()
            .Apply();

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = Alex" + Environment.NewLine);
    }

    [Test]
    public void Truncate_LengthGreaterThanStringLength_LeavesStringIntact()
    {
        var person = new SimplePerson { Name = "Sam" };
        var printer = ObjectPrinter.Setup<SimplePerson>()
            .For(p => p.Name)
                .Truncate(10)
                .Then()
            .Apply();

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = Sam" + Environment.NewLine);
    }

    [Test]
    public void CyclicReferences_HandledGracefullyWithoutStackOverflow()
    {
        var first = new Node { Name = "First" };
        var second = new Node { Name = "Second" };
        first.Next = second;
        second.Next = first;

        var result = first.PrintToString();

        result.Should().Contain("[cyclic reference]");
    }

    [Test]
    public void Collections_Array_PrintsAllElements()
    {
        int[] numbers = [10, 20, 30];

        var result = numbers.PrintToString();

        result.Should().Contain("[0] = 10");
        result.Should().Contain("[1] = 20");
        result.Should().Contain("[2] = 30");
    }

    [Test]
    public void Collections_List_PrintsAllElements()
    {
        var words = new List<string> { "hello", "world" };

        var result = words.PrintToString();

        result.Should().Contain("[0] = hello");
        result.Should().Contain("[1] = world");
    }

    [Test]
    public void Collections_Dictionary_PrintsKeysAndValues()
    {
        var dict = new Dictionary<string, int>
        {
            ["alpha"] = 1,
            ["beta"] = 2
        };

        var result = dict.PrintToString();

        result.Should().Contain("[alpha] = 1");
        result.Should().Contain("[beta] = 2");
    }

    [Test]
    public void Collections_InsideComplexObject_PrintsCorrectly()
    {
        var holder = new CollectionHolder
        {
            Numbers = [1, 2],
            Words = ["foo"],
            Map = new() { ["key"] = 42 }
        };

        var result = holder.PrintToString();

        result.Should().Contain("[0] = 1");
        result.Should().Contain("[1] = 2");
        result.Should().Contain("[0] = foo");
        result.Should().Contain("[key] = 42");
    }

    [Test]
    public void Collections_CyclicReferenceInList_DoesNotThrow()
    {
        var list = new List<object>();
        list.Add(list);

        var result = list.PrintToString();

        result.Should().Contain("[cyclic reference]");
    }

    [Test]
    public void Collections_ExcludeType_DoesNotPrintEmptyIndexEntries()
    {
        var list = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var printer = ObjectPrinter.Setup<List<Guid>>()
            .Exclude<Guid>()
            .Apply();

        var result = printer.PrintToString(list);

        result.Should().NotContain("[0] =");
    }

    [Test]
    public void SetFormat_HandlesNullSafely()
    {
        var person = new SimplePerson { Name = null! };
        var printer = ObjectPrinter.Setup<SimplePerson>()
            .For(p => p.Name)
                .SetFormat(name => name is null ? "DEFAULT" : name)
                .Then()
            .Apply();

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = DEFAULT");
    }

    [Test]
    public void PrintToString_TerminalTypes_PrintsWithoutBreakingDownInternalProperties()
    {
        var guid = Guid.Parse("11111111-2222-3333-4444-555555555555");

        var result = guid.PrintToString();

        result.Trim().Should().Be("11111111-2222-3333-4444-555555555555");
    }

    private class ThrowingPropertyModel
    {
        public string Normal { get; set; } = "Fine";
        public string Buggy => throw new InvalidOperationException("boom");
    }

    [Test]
    public void PrintToString_PropertyThrowsException_PrintsErrorGracefully()
    {
        var model = new ThrowingPropertyModel();

        var result = model.PrintToString();

        result.Should().Contain("Normal = Fine");
        result.Should().Contain("Buggy = [throws InvalidOperationException: boom]");
    }

    [Test]
    public void Collections_EmptyCollections_PrintsEmptyIndicator()
    {
        var emptyList = new List<int>();
        var emptyDict = new Dictionary<string, int>();

        var listResult = emptyList.PrintToString();
        var dictResult = emptyDict.PrintToString();

        listResult.Should().Contain("List`1 []");
        dictResult.Should().Contain("Dictionary`2 []");
    }

    [Test]
    public void Collections_InfiniteSequence_TruncatesGracefully()
    {
        static IEnumerable<int> InfiniteNumbers()
        {
            while (true)
                yield return 42;
        }

        var result = InfiniteNumbers().PrintToString();

        result.Should().Contain("[truncated]");
    }

    private interface IAnimal { string Sound { get; } }
    private class Cat : IAnimal { public string Sound => "Meow"; public string Name => "Barsik"; }
    private class Zoo { public IAnimal Pet { get; set; } = new Cat(); }

    [Test]
    public void Exclude_InterfaceOrBaseType_ExcludesDerivedInstances()
    {
        var zoo = new Zoo();
        var printer = ObjectPrinter.Setup<Zoo>()
            .Exclude<IAnimal>()
            .Apply();

        var result = printer.PrintToString(zoo);

        result.Should().NotContain("Pet =");
    }

    private class NullableHolder
    {
        public double? Value { get; set; } = 42.5;
    }

    private class ShadowBase
    {
        public string Title { get; set; } = "BaseTitle";
    }

    private class ShadowDerived : ShadowBase
    {
        public new string Title { get; set; } = "DerivedTitle";
    }

    [Test]
    public void For_NullableType_AppliesCultureAndFormatCorrectly()
    {
        var holder = new NullableHolder();
        var ruCulture = new CultureInfo("ru-RU");

        var printer = ObjectPrinter.Setup<NullableHolder>()
            .For<double?>()
                .SetCulture(ruCulture)
                .Then()
            .Apply();

        var result = printer.PrintToString(holder);

        result.Should().Contain("Value = 42,5");
    }

    [Test]
    public void PrintToString_ShadowedProperty_DoesNotDuplicate()
    {
        var derived = new ShadowDerived();

        var result = derived.PrintToString();

        result.Should().Contain("Title = DerivedTitle");
        result.Should().NotContain("Title = BaseTitle");
    }

    [Test]
    public void Collections_ReadOnlyDictionary_PrintsAsDictionaryEntries()
    {
        IReadOnlyDictionary<string, int> dict = new Dictionary<string, int>
        {
            ["first"] = 10,
            ["second"] = 20
        };

        var result = dict.PrintToString();

        result.Should().Contain("[first] = 10");
        result.Should().Contain("[second] = 20");
        result.Should().NotContain("KeyValuePair");
    }
}
