using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ConfigurationSerializationTests : TestBase
{
    [Test]
    public void PrintToString_WithExcludeType_ExcludesPropertiesOfType()
    {
        var printer = ObjectPrinter.For<Person>().ExcludeType<Guid>();
        var result = printer.PrintToString(TestPerson);

        result.Should().NotContain("Id = ").And.Contain("Name = John Doe");
    }

    [Test]
    public void PrintToString_WithExcludeProperty_ExcludesSpecificProperty()
    {
        var printer = ObjectPrinter.For<Person>().ExcludeProperty(p => p.Age);
        var result = printer.PrintToString(TestPerson);

        result.Should().NotContain("Age = 30").And.Contain("Name = John Doe");
    }

    [Test]
    public void PrintToString_WithTypeSerializer_UsesCustomSerialization()
    {
        var printer = ObjectPrinter.For<Person>().SerializeType<int>()
            .Use(i => $"Integer: {i}");
        var result = printer.PrintToString(TestPerson);

        result.Should().Contain("Age = Integer: 30");
    }

    [Test]
    public void PrintToString_WithCulture_UsesSpecifiedCulture()
    {
        var person = new Person { Height = 180.5 };
        var japaneseCulture = CultureInfo.GetCultureInfo("ja-JP");
        var printer = ObjectPrinter.For<Person>().SerializeType<double>().Use(japaneseCulture);
        var result = printer.PrintToString(person);

        result.Should().Contain("180.5");
    }

    [Test]
    public void
        PrintToString_WithPropertySerializer_UsesCustomSerializationForProperty()
    {
        var printer = ObjectPrinter.For<Person>()
            .SerializeProperty(p => p.Name)
            .Use(name => $"Name: {name.ToUpper()}");
        var result = printer.PrintToString(TestPerson);

        result.Should().Contain("Name = Name: JOHN DOE");
    }

    [Test]
    public void PrintToString_WithStringPropertyTrim_TrimsStringProperty()
    {
        var printer = ObjectPrinter.For<Person>()
            .SerializeProperty(p => p.Name).TrimTo(4);
        var result = printer.PrintToString(TestPerson);

        result.Should().Contain("Name = John");
    }

    [Test]
    public void
        PrintToString_WithMultipleConfigurations_AppliesAllConfigurations()
    {
        var printer = ObjectPrinter.For<Person>()
            .ExcludeType<Guid>()
            .ExcludeProperty(p => p.Email)
            .SerializeType<int>().Use(i => $"{i} years")
            .SerializeProperty(p => p.Name).TrimTo(4);

        var result = printer.PrintToString(TestPerson);

        result.Should()
            .NotContain("Id = ")
            .And.NotContain("Email = ")
            .And.Contain("Age = 30 years")
            .And.Contain("Name = John");
    }

    [Test]
    public void PrintToString_WithCustomTypeSerializer_PriorityOverCulture()
    {
        var printer = ObjectPrinter.For<Person>().SerializeType<double>()
            .Use(d => $"Height: {d} cm");
        var result = printer.PrintToString(TestPerson);

        result.Should().Contain("Height: 180,5 cm");
    }

    [Test]
    public void
        PrintToString_WithPropertySerializer_PriorityOverTypeSerializer()
    {
        var printer = ObjectPrinter.For<Person>()
            .SerializeType<string>().Use(s => s.ToUpper())
            .SerializeProperty(p => p.Name).Use(name => $"Mr. {name}");

        var result = printer.PrintToString(TestPerson);
        result.Should().Contain("Name = Mr. John Doe");
    }

    [Test]
    public void PrintToString_WithInvariantCulture_FormatsNumbersConsistently()
    {
        var printer = ObjectPrinter.For<Person>().SerializeType<double>()
            .Use(CultureInfo.InvariantCulture);
        var result = printer.PrintToString(TestPerson);

        result.Should().Contain("180.5");
    }

    [Test]
    public void PrintingConfig_ExcludeMultipleTypes_WorksCorrectly()
    {
        var config = ObjectPrinter.For<Person>()
            .ExcludeType<Guid>()
            .ExcludeType<string>();

        var person = new Person
            { Id = Guid.NewGuid(), Name = "Test", Age = 25 };
        var result = config.PrintToString(person);

        result.Should()
            .NotContain("Id = ")
            .And.NotContain("Name = ")
            .And.Contain("Age = 25");
    }

    [Test]
    public void PrintingConfig_ChainedConfiguration_ReturnsCorrectType()
    {
        var config = ObjectPrinter.For<Person>()
            .ExcludeType<Guid>()
            .SerializeType<int>().Use(i => i.ToString())
            .SerializeProperty(p => p.Name).Use(n => n)
            .ExcludeProperty(p => p.Age);

        config.Should().BeOfType<PrintingConfig<Person>>();
    }
    
    [Test]
    public void PrintToString_WithConfigAndNestingLevel_AppliesBoth()
    {
        var nestedObject = new NestedContainer
        {
            Value = "Level 1",
            Child = new NestedContainer
            {
                Value = "Very Long Value That Should Be Trimmed",
                Child = new NestedContainer { Value = "Level 3" }
            }
        };

        var result = nestedObject.PrintToString(
            configurator => configurator
                .SerializeProperty(x => x.Value).TrimTo(10),
            nestingLevel: 2
        );
        
        result.Should().Contain("Value = Level 1");
        result.Should().Contain("Value = Very Long");
        result.Should().NotContain("Level 3");
    }
    
    [Test]
    public void PrintingConfig_ShouldReturnNewInstance_AfterConfiguration()
    {
        var printer = ObjectPrinter.For<Person>().ExcludeType<Guid>();
        var printer2 = printer.ExcludeType<int>();
        
        printer.Should().NotBeSameAs(printer2);
    }
}