using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class CollectionSerializationTests
{
    [Test]
    public void PrintToString_WithIntArray_ShouldSerializeCorrectly()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Scores = new List<int>() { 95, 87, 92 }
        };

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Contain("Scores")
              .And.Contain("[")
              .And.Contain("95")
              .And.Contain("87")
              .And.Contain("92");
    }

    [Test]
    public void PrintToString_WithStringList_ShouldSerializeCorrectly()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Friends = new List<string> { "John", "Mike", "Sarah" }
        };

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Contain("Friends")
              .And.Contain("[")
              .And.Contain("John")
              .And.Contain("Mike")
              .And.Contain("Sarah");
    }

    [Test]
    public void PrintToString_WithEmptyCollection_ShouldHandleCorrectly()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Friends = new List<string>(),
            Scores = new List<int>()
        };

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Contain("Friends").And.Contain("Scores")
              .And.NotContain("John");
    }

    [Test]
    public void PrintToString_WithNestedLists_ShouldSerializeCorrectly()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Matrix = new List<List<int>> 
            { 
                new() { 1, 2, 3 },
                new() { 4, 5, 6 }
            }
        };

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Contain("Matrix")
              .And.Contain("1").And.Contain("2").And.Contain("3")
              .And.Contain("4").And.Contain("5").And.Contain("6");
    }
}