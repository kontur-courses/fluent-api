using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class EdgeCasesTests
{
    [Test]
    public void PrintToString_WithNullObject_ShouldReturnNull()
    {
        Person person = null;

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Be("null" + Environment.NewLine);
    }

    [Test]
    public void PrintToString_WithNullProperties_ShouldHandleCorrectly()
    {
        var person = new Person { Name = null, Friends = null };

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = null").And.Contain("Friends = null");
    }

    [Test]
    public void ConfigureNonExistentProperty_ShouldThrowException()
    {
        var printer = ObjectPrinter.For<Person>();

        Action act = () => printer.ConfigureProperty(p => "invalid");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void MultipleConfigurations_ForSameProperty_ShouldUseLastOne()
    {
        var person = new Person { Age = 25 };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureProperty(p => p.Age).WithSerialization(age => $"First: {age}")
            .ConfigureProperty(p => p.Age).WithSerialization(age => $"Second: {age}");

        var result = printer.PrintToString(person);

        result.Should().Contain("Second: 25").And.NotContain("First: 25");
    }
}