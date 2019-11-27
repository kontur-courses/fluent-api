using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private readonly Person person = new Person {Name = "Alex", Age = 19, Height = 179.9};

        [Test]
        public void ExcludingTypeShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            printer.PrintToString(person).Should().NotContain("19").And.Contain("Alex");
        }

        [Test]
        public void ExcludingPropertyShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Name);

            printer.PrintToString(person).Should().NotContain("Alex").And.NotContain("Name");
        }

        [Test]
        public void UsingWithAlternativeSerializationMethodShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => i.ToString("X"));

            printer.PrintToString(person).Should().NotContain("19").And.Contain("13");
        }

        [Test]
        public void UsingWithCultureShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("en"));

            printer.PrintToString(person).Should().NotContain("179,9").And.Contain("179.9");
        }

        [Test]
        public void TrimmedToLengthShouldWorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(2);

            printer.PrintToString(person).Should().Contain("Al").And.NotContain("Alex");
        }

        [Test]
        public void PrintToStringShouldWorkCorrectOnObject()
        {
            person.PrintToString().Should().Contain("Name = Alex")
                .And.Contain("Age = 19")
                .And.Contain("Height = 179,9")
                .And.Contain("Id");
        }

        [Test]
        public void PrintToStringWithConfigShouldWorkCorrectOnObject()
        {
            person.PrintToString(config => config.Excluding(p => p.Name))
                .Should().NotContain("Alex");
        }

        [Test]
        public void PrintToStringShouldNotPrintPrivateMembers()
        {
            person.PrintToString()
                .Should().NotContain("budget");
        }

        [Test]
        public void PrintToStringWithConfigShouldWorkCorrectOnArray()
        {
            var arr = new[] {1, 2, 3};
            arr.PrintToString().Should().Contain("Int32[]")
                .And.Contain("1\r\n2\r\n3\r\n");
        }

        [Test]
        public void PrintToStringWithConfigShouldWorkCorrectOnList()
        {
            var list = new List<string> {"a", "b", "c"};
            list.PrintToString().Should().Contain("List<String>")
                .And.Contain("a\r\nb\r\nc\r\n");
        }


        [Test]
        public void PrintToStringWithConfigShouldWorkCorrectOnDictionary()
        {
            var dictionary = new Dictionary<string, int>()
            {
                ["a"] = 1,
                ["b"] = 2
            };
            dictionary.PrintToString().Should().Contain("Dictionary<String, Int32>")
                .And.Contain("KeyValuePair<String, Int32>")
                .And.Contain("Key = a")
                .And.Contain("Value = 1")
                .And.Contain("Key = b")
                .And.Contain("Value = 2");
        }

        [Test]
        public void PrintToStringShouldWorkCorrectWhenObjectHaveCyclicReferences()
        {
            var firstPerson = new Person() {Age = 20, Height = 190, Name = "Bob"};
            var secondPerson = new Person() {Age = 19, Height = 170, Name = "Alice"};
            firstPerson.Friends = new[] {secondPerson};
            secondPerson.Friends = new[] {firstPerson};
            Action act = () => firstPerson.PrintToString();
            act.ShouldNotThrow<StackOverflowException>();
        }
    }
}