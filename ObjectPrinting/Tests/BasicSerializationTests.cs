using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class BasicSerializationTests
{
    [Test]
    public void ExcludeType_ShouldExcludeAllPropertiesOfType()
    {
        var person = new Person { Name = "Alex", Surname = "Ivanov",Age = 19, Height = 180.5, BirthDate = DateTime.Now };

        var printer = ObjectPrinter.For<Person>()
            .ExcludeType<string>()
            .ExcludeType<DateTime>();

        var result = printer.PrintToString(person);

        result.Should().NotContain("Name").And.NotContain("BirthDate").And.NotContain("Surname")
              .And.Contain("Age").And.Contain("Height");
    }

    [Test]
    public void ExcludeProperty_ShouldExcludeSpecificProperty()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 180.5 };

        var printer = ObjectPrinter.For<Person>()
            .ExcludeProperty(p => p.Height)
            .ExcludeProperty(p => p.Age);

        var result = printer.PrintToString(person);

        result.Should().Contain("Name")
              .And.NotContain("Height").And.NotContain("Age");
    }

    [Test]
    public void ConfigureType_WithCustomSerialization_ShouldApplyToAllPropertiesOfType()
    {
        var person = new Person { Age = 19, YearOfBirth = 2004, Height = 180 };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureType<int>().WithSerialization(i => $"#{i}#")
            .ConfigureType<double>().WithSerialization(d => $"{d:0.0}cm");

        var result = printer.PrintToString(person);

        result.Should().Contain("Age = #19#")
              .And.Contain("YearOfBirth = #2004#")
              .And.Contain("Height = 180,0cm");
    }

    [Test]
    public void ConfigureProperty_WithCustomSerialization_ShouldApplyToSpecificProperty()
    {
        var person = new Person { Name = "Alex", Age = 19, YearOfBirth = 2004 };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureProperty(p => p.Age).WithSerialization(age => $"Возраст: {age}")
            .ConfigureProperty(p => p.Name).WithSerialization(name => $"Имя: {name}");

        var result = printer.PrintToString(person);

        result.Should().Contain("Age = Возраст: 19")
              .And.Contain("Name = Имя: Alex")
              .And.Contain("YearOfBirth = 2004");
    }
}