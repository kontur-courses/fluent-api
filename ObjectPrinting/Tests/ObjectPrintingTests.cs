using System;
using NUnit.Framework;
using FluentAssertions;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;


namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrintingTests
{
    public class TestAddress
    {
        public string City { get; set; } = "";
    }

    public class TestWeight
    {
        public double Weight { get; set; } = 1.5;
    }

    public class TestPerson
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public TestAddress? Address { get; set; }
    }

    public class TestNode
    {
        public string Name { get; set; } = "";
        public TestNode? Next { get; set; }
    }

    public class TestScores
    {
        public int Score { get; set; }
        public int Bonus;
    }

    [Test]
    public void ExcludingType_RemovesPropertiesAndFieldsOfThatType()
    {
        var scores = new TestScores { Score = 26, Bonus = 31 };

        var result = ObjectPrinter.For<TestScores>().Excluding<int>().PrintToString(scores);

        result.Should().Be("TestScores" + Environment.NewLine);
    }

    [Test]
    public void PrintingType_FormatsPropertiesAndFieldsOfThatType()
    {
        var scores = new TestScores { Score = 26, Bonus = 31 };

        var result = ObjectPrinter.For<TestScores>()
            .Printing<int>().Using(value => value.ToString("X"))
            .PrintToString(scores);

        result.Should().Be("TestScores" + Environment.NewLine + "\tScore = 1A" + Environment.NewLine + "\tBonus = 1F" +
                           Environment.NewLine);
    }
    
    [Test]
    public void ObjectPrinter_PrintsArrayElements()
    {
        var colors = new[] { "red", "green", "blue" };

        var result = colors.PrintToString();

        result.Should().Contain("red").And.Contain("green").And.Contain("blue");
    }
    
    [Test]
    public void ObjectPrinter_PrintsNestedList()
    {
        var team = new
        {
            Members = new List<object?>
            {
                new { Name = "Alice" },
                null
            }
        };

        var result = team.PrintToString();

        result.Should().Contain("Members = ");
        result.Should().Contain("Name = Alice");
        result.Should().Contain("null");
    }
    
    [Test]
    public void ExcludingType_RemovesAllPropertiesOfThatType()
    {
        var person = new TestPerson { Name = "Alex", Age = 19 };

        var result = ObjectPrinter.For<TestPerson>().Excluding<int>().PrintToString(person);

        result.Should().Be("TestPerson" + Environment.NewLine + "\tName = Alex" + Environment.NewLine +
                           "\tAddress = null" + Environment.NewLine);
    }

    [Test]
    [SetCulture("en-US")]
    public void PrintingType_UsesConfiguredCulture()
    {
        var item = new TestWeight { Weight = 1.5 };

        var result = ObjectPrinter.For<TestWeight>()
            .Printing<double>().Using(CultureInfo.GetCultureInfo("fr-FR"))
            .PrintToString(item);

        result.Should().Be("TestWeight" + Environment.NewLine + "\tWeight = 1,5" + Environment.NewLine);
    }

    [Test]
    public void PrintingMember_OverridesTypeFormatterForSelectedField()
    {
        var scores = new TestScores { Score = 26, Bonus = 31 };

        var result = ObjectPrinter.For<TestScores>().Printing<int>().Using(value => value.ToString("X"))
            .Printing(s => s.Bonus).Using(value => value + " points").PrintToString(scores);

        result.Should().Be("TestScores" + Environment.NewLine + "\tScore = 1A" + Environment.NewLine +
                           "\tBonus = 31 points" + Environment.NewLine);
    }
    
    [Test]
    public void TrimmingString_CutsValueToMaxLength()
    {
        var person = new TestPerson { Name = "Alexandra", Age = 19 };

        var result = ObjectPrinter.For<TestPerson>().Printing(p => p.Name).TrimmedToLength(4).PrintToString(person);

        result.Should().Be("TestPerson" + Environment.NewLine + "\tName = Alex" + Environment.NewLine + "\tAge = 19" + Environment.NewLine + "\tAddress = null" + Environment.NewLine);
    }
    
    public static IEnumerable HappyPathCases
    {
        get
        {
            yield return new TestCaseData<TestPerson, string>(new TestPerson { Name = "Alex", Age = 20 },
                    "TestPerson" + Environment.NewLine + "\tName = Alex" + Environment.NewLine + "\tAge = 20" +
                    Environment.NewLine + "\tAddress = null" + Environment.NewLine)
                .SetName("HappyPath: simple object prints all public properties");

            yield return new TestCaseData<TestPerson, string>(
                new TestPerson { Name = "Alex", Age = 20, Address = new TestAddress { City = "Paris" } },
                "TestPerson" + Environment.NewLine + "\tName = Alex" + Environment.NewLine + "\tAge = 20" +
                Environment.NewLine + "\tAddress = TestAddress" + Environment.NewLine + "\t\tCity = Paris" +
                Environment.NewLine).SetName("HappyPath: nested object is printed with extra indentation");
        }
    }


    public static IEnumerable CycleCases
    {
        get
        {
            var self = new TestNode { Name = "A" };
            self.Next = self;

            yield return new TestCaseData<TestNode, string>(self,
                "TestNode" + Environment.NewLine + "\tName = A" + Environment.NewLine + "\tNext = <Cycle>" +
                Environment.NewLine).SetName("Cycle: object referencing to itself");

            var a = new TestNode { Name = "A" };
            var b = new TestNode { Name = "B" };
            var c = new TestNode { Name = "C" };
            a.Next = b;
            b.Next = c;
            c.Next = a;

            yield return new TestCaseData<TestNode, string>(a,
                "TestNode" + Environment.NewLine + "\tName = A" + Environment.NewLine + "\tNext = TestNode" +
                Environment.NewLine + "\t\tName = B" + Environment.NewLine + "\t\tNext = TestNode" +
                Environment.NewLine + "\t\t\tName = C" + Environment.NewLine + "\t\t\tNext = <Cycle>" +
                Environment.NewLine).SetName("Cycle: three objects forming a loop");
        }
    }

    [TestCaseSource(nameof(HappyPathCases))]
    public void ObjectPrinter_PrintsObjects(TestPerson obj, string expected)
    {
        var result = obj.PrintToString();

        result.Should().Be(expected);
    }

    [TestCaseSource(nameof(CycleCases))]
    public void ObjectPrinter_HandlesCycles(TestNode obj, string expected)
    {
        var result = obj.PrintToString();

        result.Should().Be(expected);
    }
}