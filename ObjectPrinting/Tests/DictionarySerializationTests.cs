using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class DictionarySerializationTests
{
    [Test]
    public void PrintToString_WithStringIntDictionary_ShouldSerializeCorrectly()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Grades = new Dictionary<string, int> 
            { 
                ["Math"] = 95, 
                ["Science"] = 87 
            }
        };

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Contain("Grades")
            .And.Contain("{")
            .And.Contain("Math: 95")
            .And.Contain("Science: 87");
    }

    [Test]
    public void PrintToString_WithComplexValueDictionary_ShouldSerializeCorrectly()
    {
        var person = new Person 
        { 
            Name = "Alex",
            SubjectDetails = new Dictionary<string, Subject> 
            { 
                ["Math"] = new() { Score = 95, Teacher = "Mr. Smith" },
                ["Science"] = new() { Score = 87, Teacher = "Ms. Johnson" }
            }
        };

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Contain("SubjectDetails")
            .And.Contain("Math")
            .And.Contain("Science")
            .And.Contain("Mr. Smith")
            .And.Contain("Ms. Johnson");
    }

    [Test]
    public void PrintToString_WithEmptyDictionary_ShouldHandleCorrectly()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Grades = new Dictionary<string, int>()
        };

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Contain("Grades").And.NotContain("Math");
    }
}

public class Subject
{
    public int Score { get; set; }
    public string Teacher { get; set; }
}