using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ObjectPrinterTests
{
    private Person person;

    [SetUp]
    public void SetUp()
    {
        person = new Person()
        {
            Id = Guid.NewGuid(),
            Age = 18,
            Height = 170,
            Name = "Bob",
            weight = 75
        };
    }

    [Test]
    public void PrintToStringDefault_ShouldPrintDefaultValues()
    {
        var expected = new StringBuilder()
            .AppendLine(nameof(Person))
            .AppendLine($"\t{nameof(Person.Id)} = {nameof(Guid)}")
            .AppendLine($"\t{nameof(Person.Name)} = {person.Name}")
            .AppendLine($"\t{nameof(Person.Height)} = {person.Height}")
            .AppendLine($"\t{nameof(Person.Age)} = {person.Age}")
            .AppendLine($"\t{nameof(Person.Parent)} = null")
            .AppendLine($"\t{nameof(Person.Child)} = null")
            .AppendLine($"\t{nameof(Person.Items)} = null")
            .AppendLine($"\t{nameof(Person.SignificantEvents)} = null")
            .AppendLine($"\t{nameof(Person.weight)} = {person.weight}")
            .ToString();

        var serialized = person.PrintToStringDefault();

        serialized.Should().Be(expected);
    }

    [Test]
    public void Excluding_ShouldExcludeSelectedType()
    {
        var config = ObjectPrinter.For<Person>().Excluding<int>();

        var serialized = config.PrintToString(person);

        serialized.Should().NotContainAll($"{nameof(Person.weight)}", $"{nameof(Person.Age)}");
    }

    [Test]
    public void SerializeUsing_ShouldApplyFormattingOfType()
    {
        var config = ObjectPrinter.For<Person>().Serialize<string>().Using(value => $"{value}{value}");

        var serialized = config.PrintToString(person);

        serialized.Should().Contain($"{nameof(Person.Name)} = {person.Name}{person.Name}");
    }

    [Test]
    public void SerializeUsing_ShouldApplyCultureFormattingOfType()
    {
        var culture = new CultureInfo("de-DE");
        var config = ObjectPrinter.For<Person>()
            .Serialize<double>().Using(culture);

        var serialized = config.PrintToString(person);

        serialized.Should().Contain($"{nameof(Person.Height)} = {person.Height.ToString(culture)}");
    }

    [Test]
    public void SerializeUsing_ThrowArgumentNullException_OnNullCultureInfo()
    {
        var act = () => ObjectPrinter.For<Person>()
            .Serialize<double>().Using((IFormatProvider)null);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void SerializeUsing_ShouldApplyPropertyFormatting()
    {
        var config = ObjectPrinter.For<Person>()
            .Serialize(p => p.Age).Using(age => $"{age} years");

        var serialized = config.PrintToString(person);

        serialized.Should().Contain($"{nameof(Person.Age)} = {person.Age} years");
    }

    [Test]
    public void When_Use_ShouldApplyMemberFormatting()
    {
        var config = ObjectPrinter.For<Person>()
            .Serialize(p => p.weight).Using(weight => $"{weight} kg");

        var serialized = config.PrintToString(person);

        serialized.Should().Contain($"{nameof(Person.weight)} = {person.weight} kg");
    }

    [TestCase(0)]
    [TestCase(2)]
    [TestCase(10000)]
    public void When_UseSubstring_ShouldTakeSubstring(int length)
    {
        var config = ObjectPrinter.For<Person>()
            .Serialize<string>().TrimmedToLength(length);

        var serialized = config.PrintToString(person);

        serialized.Should().Contain($"{nameof(Person.Name)} = {person.Name[..Math.Min(length, person.Name.Length)]}");
    }

    [Test]
    public void When_UseSubstring_ShouldThrowException_WhenTrimmingLengthIsNegative()
    {
        Action act = () => ObjectPrinter.For<Person>().Serialize<string>().TrimmedToLength(-1);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Excluding_ShouldExcludeProperty()
    {
        var config = ObjectPrinter.For<Person>()
            .Excluding(p => p.Height);

        var serialized = config.PrintToString(person);

        serialized.Should().NotContain($"{nameof(Person.Height)} = {person.Height}");
    }

    [Test]
    public void IgnoringCyclicReferences_ShouldAllowCyclicReferences()
    {
        var parent = new Person()
        {
            weight = 90,
            Age = 60,
            Height = 180,
            Id = Guid.NewGuid(),
            Name = "Tom",
        };
        person.Parent = parent;
        parent.Child = person;
        var config = ObjectPrinter.For<Person>()
            .IgnoringCyclicReferences();

        var serialized = config.PrintToString(person);

        serialized.Should().Contain($"{nameof(parent.Child)} = {{...}}");
    }

    [Test]
    public void PrintToString_ShouldThrowInvalidOperationException_OnCyclicReference()
    {
        var parent = new Person()
        {
            weight = 90,
            Age = 60,
            Height = 180,
            Id = Guid.NewGuid(),
            Name = "Tom",
        };
        person.Parent = parent;
        parent.Child = person;
        var config = ObjectPrinter.For<Person>();

        Action act = () => config.PrintToString(person);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void PrintToObject_ShouldHandleArrays()
    {
        person.Items = new[] { "Книга", "Ручка" };
        var config = ObjectPrinter.For<Person>();

        var serialized = config.PrintToString(person);

        serialized.Should()
            .ContainAll($"{nameof(Person.Items)} = ", "[", "Книга", "Ручка", "]");
    }

    [Test]
    public void PrintToString_ShouldHandleArray_OnEmptyArray()
    {
        person.Items = Array.Empty<string>();
        var config = ObjectPrinter.For<Person>();

        var serialized = config.PrintToString(person);

        serialized.Should().Contain($"{nameof(Person.Items)} = []");
    }

    [Test]
    public void PrintToString_ShouldHandleList()
    {
        person.Items = new List<string> { "Книга", "Ручка" };
        var config = ObjectPrinter.For<Person>();

        var serialized = config.PrintToString(person);

        serialized.Should()
            .ContainAll($"{nameof(Person.Items)} = ", "[", "Книга", "Ручка", "]");
    }

    [Test]
    public void PrintToString_ShouldHandleList_OnEmptyList()
    {
        person.Items = new List<string>();
        var config = ObjectPrinter.For<Person>();

        var serialized = config.PrintToString(person);

        serialized.Should().Contain($"{nameof(Person.Items)} = []");
    }
    
    [Test]
    public void PrintToString_ShouldHandleDictionary()
    {
        person.SignificantEvents = new Dictionary<int,string>
        {
            [2025] = "Купил дом",
            [2030] = "Купил квартиру"
        };
        var config = ObjectPrinter.For<Person>();

        var serialized = config.PrintToString(person);

        serialized.Should().ContainAll($"{nameof(Person.SignificantEvents)} = ", "[", "KeyValuePair`2", "Key = 2025",
            "Value = Купил дом", "KeyValuePair`2", "Key = 2030", "Value = Купил квартиру", "]");
    }

    [Test]
    public void PrintToString_ShouldHandleDictionary_OnEmptyDictionary()
    {
        person.SignificantEvents = new Dictionary<int,string>();
        var config = ObjectPrinter.For<Person>();

        var serialized = config.PrintToString(person);

        serialized.Should().Contain($"{nameof(Person.SignificantEvents)} = []");
    }
}