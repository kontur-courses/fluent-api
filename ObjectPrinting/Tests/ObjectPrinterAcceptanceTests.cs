using FluentAssertions;
using NUnit.Framework;
using System;
using static System.Globalization.CultureInfo;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    private const int TrimmedLength = 2;

    private Person _person = null!;
    private IPrintingConfig<Person> _printer = null!;

    [SetUp]
    public void SetUp()
    {
        _person = new Person
        {
            Id = Guid.NewGuid(),
            Name = "Mikasa",
            Age = 19,
            Height = 1.76,
            Gender = "Female"
        };

        _printer = ObjectPrinter.For<Person>();
    }

    [Test]
    public void PrintToStringExtension_DefaultConfiguration_DefaultSerialization()
    {
        var result = _person.PrintToString();

        result.Should().Contain("Mikasa");
        result.Should().Contain("19");
        result.Should().Contain("1,76");
    }

    [Test]
    public void PrintToStringExtension_WithCustomConfiguration_ApplyCustomConfiguration()
    {
        var result = _person.PrintToString(cfg => cfg.Printing(p => p.Name).TrimmedToLength(TrimmedLength).Printing(p => p.Height).UsingCulture(InvariantCulture));

        result.Should().Contain("Mi");
        result.Should().Contain("1.76");
    }

    [Test]
    public void PrintToString_ExcludingType_SkipAllMembersOfThisType()
    {
        _printer = _printer.Excluding<string>();

        var result = _printer.PrintToString(_person);

        result.Should().NotContain(nameof(_person.Name));
        result.Should().NotContain(nameof(_person.Gender));
    }

    [Test]
    public void PrintToString_UsingCustomSerializerForType_ApplySerializerToAllMembersOfType()
    {
        _printer = _printer.Printing<int>().UsingSerializer(i => $"Целое число: {i}");

        var result = _printer.PrintToString(_person);

        result.Should().Contain("Целое число: 19");
    }

    [Test]
    public void PrintToString_SetCultureForNumericType_ApplyCultureToSerialization()
    {
        _printer = _printer.Printing<double>().UsingCulture(InvariantCulture);

        var result = _printer.PrintToString(_person);

        result.Should().Contain("1.76");
    }

    [Test]
    public void PrintToString_CustomSerializerForProperty_ChangeOnlyThisProperty()
    {
        _printer = _printer.Printing(p => p.Age).UsingSerializer(a => $"Возраст человека: {a}");

        var result = _printer.PrintToString(_person);

        result.Should().Contain("Возраст человека: 19");
    }

    [Test]
    public void PrintToString_TrimProperty_TrimPropertyCorrectly()
    {
        _printer = _printer.Printing(p => p.Name).TrimmedToLength(TrimmedLength);

        var result = _printer.PrintToString(_person);

        result.Should().Contain("Mi");
    }

    [Test]
    public void PrintToString_ExcludeProperty_SkipThisProperty()
    {
        _printer = _printer.Excluding(p => p.Height);

        var result = _printer.PrintToString(_person);

        result.Should().NotContain("Height");
    }

    [Test]
    public void PrintToString_CyclicReference_ShouldNotCallStackOverflow()
    {
        var parent = new Person { Name = "Grisha" };
        var child = new Person { Name = "Eren" };

        parent.Child = child;
        child.Child = parent;

        var result = _printer.PrintToString(parent);

        result.Should().Contain("cyclic reference");
    }
}