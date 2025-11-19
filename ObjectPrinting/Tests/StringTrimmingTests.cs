using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class StringTrimmingTests
{
    [Test]
    public void ConfigureType_Trim_ShouldTrimAllStringProperties()
    {
        var person = new Person { Name = "Alexander", Description = "Very long description" };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureType<string>().Trim(0, 5);

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = Alex").And.Contain("Description = Very ");
    }

    [Test]
    public void ConfigureProperty_Trim_ShouldTrimOnlySpecificProperty()
    {
        var person = new Person { Name = "Alexander", Description = "Very long description" };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureProperty(p => p.Name).Trim(0, 4)
            .ConfigureProperty(p => p.Description).Trim(0, 7);

        var result = printer.PrintToString(person);

        result.Should().Contain("Name = Alex").And.Contain("Description = Very lo");
    }

    [Test]
    public void Trim_WithVariousParameters_ShouldWorkCorrectly()
    {
        var person = new Person { Name = "Alexander" };

        var printer1 = ObjectPrinter.For<Person>()
            .ConfigureProperty(p => p.Name).Trim(2, 6);
        var result1 = printer1.PrintToString(person);

        var printer2 = ObjectPrinter.For<Person>()
            .ConfigureProperty(p => p.Name).Trim(0, 100); 
        var result2 = printer2.PrintToString(person);

        result1.Should().Contain("Name = exa");
        result2.Should().Contain("Name = Alexander");
    }
}