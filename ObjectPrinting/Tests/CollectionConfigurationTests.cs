using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class CollectionConfigurationTests
{
    [Test]
    public void ConfigureCollection_WithMaxItems_ShouldLimitCollectionOutput()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Friends = new List<string> { "John", "Mike", "Sarah", "Tom", "Anna" }
        };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureCollection<List<string>>()
                .WithMaxItems(3);

        var result = printer.PrintToString(person);

        result.Should().Contain("John")
              .And.Contain("Mike")
              .And.Contain("Sarah")
              .And.Contain("... and 2 more items")
              .And.NotContain("Tom")
              .And.NotContain("Anna");
    }

    [Test]
    public void ConfigureCollection_WithItemSerialization_ShouldApplyToCollectionItems()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Scores = new List<int> { 95, 87, 92 }
        };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureCollection<List<int>>()
                .WithItemSerialization(score => $"Score: {score}");

        var result = printer.PrintToString(person);

        result.Should().Contain("Score: 95")
              .And.Contain("Score: 87")
              .And.Contain("Score: 92");
    }

    [Test]
    public void ConfigureCollection_TrimStrings_ShouldTrimStringItems()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Friends = new List<string> { "Alexander", "Michael", "Jennifer" }
        };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureCollection<List<string>>()
                .Trim(0, 4);

        var result = printer.PrintToString(person);

        result.Should().Contain("Alex")
              .And.Contain("Mich")
              .And.Contain("Jenn");
    }

    [Test]
    public void ConfigureCollection_WithCulture_ShouldApplyCultureToNumericItems()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Prices = new List<decimal> { 19.99m, 29.50m, 5.75m }
        };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureCollection<List<decimal>>()
            .WithItemSerialization(price => ((decimal)price).ToString("C", new CultureInfo("en-US")));

        var result = printer.PrintToString(person);

        result.Should().Contain("$19.99")
              .And.Contain("$29.50")
              .And.Contain("$5.75");
    }

    [Test]
    public void ConfigureCollection_ForArrays_ShouldWork()
    {
        var person = new Person 
        { 
            Name = "Alex",
            ScoresArray = new[] { 95, 87, 92 }
        };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureCollection<int[]>()
                .WithMaxItems(2);

        var result = printer.PrintToString(person);

        result.Should().Contain("95").And.Contain("87")
              .And.Contain("... and 1 more items")
              .And.NotContain("92");
    }

    [Test]
    public void ConfigureCollection_ForDictionaries_ShouldWork()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Grades = new Dictionary<string, int> 
            { 
                ["Math"] = 95, 
                ["Science"] = 87,
                ["History"] = 92,
                ["Art"] = 78
            }
        };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureCollection<Dictionary<string, int>>()
            .WithMaxItems(2);

        var result = printer.PrintToString(person);

        result.Should().Contain("Math: 95").And.Contain("Science: 87")
            .And.Contain("... and 2 more items")
            .And.NotContain("History").And.NotContain("Art");
    }
}