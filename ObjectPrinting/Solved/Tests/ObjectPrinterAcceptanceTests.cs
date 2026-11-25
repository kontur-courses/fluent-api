using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Excluding_ShouldExcludeField_WhenFieldTypeMatches()
        {
            var person = new Person { Name = "Dima", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Excluding<float>();

            var actual = printer.PrintToString(person);

            actual.Should().NotContain("Score =").And.Contain("Age = 19");
        }

        [Test]
        public void Excluding_ShouldExcludeProperty_WhenPropertyTypeMatches()
        {
            var person = new Person { Name = "Dima", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            var actual = printer.PrintToString(person);

            actual.Should().NotContain("Age =").And.Contain("Score = 42");
        }

        [Test]
        public void Printing_ShouldUseCustomSerializer_WhenPropertyTypeMatches()
        {
            var person = new Person { Name = "Dima", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(age => age.ToString("X"));

            var actual = printer.PrintToString(person);

            actual.Should().Contain("Age = 13").And.Contain("Score = 42");
        }

        [Test]
        public void Printing_ShouldUseCustomSerializer_WhenFieldTypeMatches()
        {
            var person = new Person { Name = "Dima", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Printing<float>().Using(score => $"[{score}]");

            var actual = printer.PrintToString(person);

            actual.Should().Contain("Score = [42]").And.Contain("Age = 19");
        }

        [TestCase("en-US", "1.75")]
        [TestCase("ru-RU", "1,75")]
        public void Printing_ShouldUseCulture_WhenFormattingNumber(string cultureName, string expectedHeight)
        {
            var person = new Person { Name = "Dima", Height = 1.75 };
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.GetCultureInfo(cultureName));

            var actual = printer.PrintToString(person);

            actual.Should().Contain($"Height = {expectedHeight}");
        }

        [Test]
        public void Printing_ShouldUseCulture_WhenFormattingDate()
        {
            var date = new DateOnly(2006, 10, 28);
            var printer = ObjectPrinter.For<DateOnly>().Printing<DateOnly>().Using(CultureInfo.InvariantCulture);

            var actual = printer.PrintToString(date);

            actual.Should().Be("10/28/2006" + Environment.NewLine);
        }

        [Test]
        public void Printing_ShouldPreferProperty_WhenTypeFormatterIsAlsoConfigured()
        {
            var person = new Person { Name = "Dima", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(age => $"{age} years")
                .Printing<int>().Using(age => $"[{age}]");

            var actual = printer.PrintToString(person);

            actual.Should().Contain("Age = 19 years");
        }

        [Test]
        public void Printing_ShouldUseFieldCulture_WhenFieldIsSelected()
        {
            var person = new Person { Name = "Dima", Score = 42.5f };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Score).Using(CultureInfo.InvariantCulture);

            var actual = printer.PrintToString(person);

            actual.Should().Contain("Score = 42.5");
        }

        [Test]
        public void Printing_ShouldUseSeparateFormatters_ForMembersInDifferentBranches()
        {
            var value = new
            {
                Left = new Person { Name = "Dima" },
                Right = new Person { Name = "Sonya" }
            };

            var actual = value.PrintToString(config => config
                .Printing(p => p.Left.Name).Using(name => $"[{name}]")
                .Printing(p => p.Right.Name).Using(name => $"({name})"));

            actual.Should().Contain("Name = [Dima]").And.Contain("Name = (Sonya)");
        }

        [TestCase("Alexander", 4, "Alex")]
        [TestCase("Dima", 10, "Dima")]
        [TestCase("Dima", 0, "")]
        public void TrimmedToLength_ShouldTrimmedString_WhenMaximumLengthIsConfigured(
            string name, int maxLen, string expectedName)
        {
            var person = new Person { Name = name };
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(maxLen);

            var actual = printer.PrintToString(person);

            actual.Should().Contain($"Name = {expectedName}{Environment.NewLine}");
        }

        [Test]
        public void Excluding_ShouldExcludeOnlySelectedProperty_WhenPropertiesHaveSameType()
        {
            var value = new { Age = 19, Score = 42 };

            var actual = value.PrintToString(config => config.Excluding(p => p.Age));

            actual.Should().NotContain("Age =").And.Contain("Score = 42");
        }

        [Test]
        public void Excluding_ShouldExcludeOnlySelectedProperty_WhenBranchesHaveSameType()
        {
            var value = new
            {
                Left = new Person { Name = "Dima", Age = 19 },
                Right = new Person { Name = "Sonya" }
            };

            var actual = value.PrintToString(config => config.Excluding(p => p.Left.Name));

            actual.Should().NotContain("Name = Dima").And.Contain("Name = Sonya").And.Contain("Age = 19");
        }

        [Test]
        public void Excluding_ShouldExcludeField_WhenFieldIsSelected()
        {
            var person = new Person { Name = "Dima", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Score);

            var actual = printer.PrintToString(person);

            actual.Should().NotContain("Score =").And.Contain("Age = 19");
        }

        [Test]
        public void PrintToString_ShouldPrintParent_WhenCycleExists()
        {
            var first = new Person { Name = "first" };
            var second = new Person { Name = "second", Friend = first };
            first.Friend = second;
            var printer = ObjectPrinter.For<Person>();

            var actual = printer.PrintToString(first);

            Console.WriteLine(actual);
            actual.Should().Contain("Name = second").And.Contain("Friend = Parent");
        }

        [Test]
        public void PrintToString_ShouldPrintIndexedItems_WhenValueIsArray()
        {
            var values = new[] { "first", "second" };

            var actual = values.PrintToString();

            actual.Should().Contain("[0] = first").And.Contain("[1] = second");
        }

        [Test]
        public void PrintToString_ShouldPrintListItems_WhenObjectContainsList()
        {
            var group = new
            {
                People = new List<Person>
                {
                    new Person { Name = "Dima" },
                    new Person { Name = "Sonya" }
                }
            };

            var actual = group.PrintToString();

            actual.Should().Contain("Name = Dima").And.Contain("Name = Sonya");
        }

        [Test]
        public void PrintToString_ShouldPrintKeyValuePairs_WhenValueIsDictionary()
        {
            var ages = new Dictionary<string, int> { ["Dima"] = 19, ["Sonya"] = 20 };

            var actual = ages.PrintToString();

            actual.Should().Contain("[Dima] = 19").And.Contain("[Sonya] = 20");
        }
    }
}