using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Core;
using ObjectPrinting.Core.PropertyPrintingConfig;
using ObjectPrinting.Infrastructure;
using ObjectPrintingTests.Infrastructure;

namespace ObjectPrintingTests.Tests
{
    public class ObjectPrinterTests
    {
        private readonly Person person = new Person(
            1f, "Alex", 179.9, 19, null);

        [Test]
        public void ExcludingType_ShouldExcludePropertiesOfGivenType()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 179,9" +
                               "\r\n\tFriends = null\r\n");
        }

        [Test]
        public void ExcludingProperty_ShouldExcludeGivenProperty()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Name);

            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\n\tId = 1\r\n\tHeight = 179,9" +
                               "\r\n\tAge = 19\r\n\tFriends = null\r\n");
        }

        [Test]
        public void Using_ShouldSerializePropertyByGivenMethod_WhenAlternativeSerializationMethodIsGiven()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => i.ToString("X"));

            var actual = printer.PrintToString(person);

            actual.Should().Be("Person\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 179,9" +
                               "\r\n\tAge = 13\r\n\tFriends = null\r\n");
        }

        [TestCase("en", TestName = "EN culture",
            ExpectedResult = "Person\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 179.9" +
                             "\r\n\tAge = 19\r\n\tFriends = null\r\n")]
        [TestCase("ru", TestName = "RU culture",
            ExpectedResult = "Person\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 179,9" +
                             "\r\n\tAge = 19\r\n\tFriends = null\r\n")]
        public string Using_ShouldSerializePropertyWithGivenCulture_WhenCultureIsGiven(string culture)
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo(culture));

            return printer.PrintToString(person);
        }

        [TestCase(2, TestName = "When maxLen is positive",
            ExpectedResult = "Person\n\tId = 1\r\n\tName = Al\r\n\tHeight = 179,9" +
                             "\r\n\tAge = 19\r\n\tFriends = null\r\n")]
        [TestCase(0, TestName = "When maxLen is zero",
            ExpectedResult = "Person\n\tId = 1\r\n\tName = \r\n\tHeight = 179,9" +
                             "\r\n\tAge = 19\r\n\tFriends = null\r\n")]
        public string TrimmedToLength_ShouldReturnTrimmedStringProperty(int maxLen)
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(maxLen);

            return printer.PrintToString(person);
        }

        [Test]
        public void TrimmedToLength_ShouldThrowArgumentException_WhenMaxLenIsNegative()
        {
            Action act = () => ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(-1);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void PrintToString_ShouldPrintAllProperties_WhenCalledOnObjectAndNoParametersIsGiven()
        {
            var actual = person.PrintToString();

            actual.Should().Be("Person\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 179,9" +
                               "\r\n\tAge = 19\r\n\tFriends = null\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintAllPropertiesByConfig_WhenCalledOnObjectAndConfigIsGiven()
        {
            var actual = person.PrintToString(config => config.Excluding(p => p.Name));

            actual.Should().Be("Person\n\tId = 1\r\n\tHeight = 179,9" +
                               "\r\n\tAge = 19\r\n\tFriends = null\r\n");
        }

        [Test]
        public void PrintToString_ShouldNotPrintPrivateMembers_WhenCalledOnObject()
        {
            var actual = person.PrintToString();

            actual.Should().NotContain("budget");
        }

        [Test]
        public void PrintToString_ShouldPrintElements_WhenCalledOnArray()
        {
            var arr = new[] {1, 2, 3};

            var actual = arr.PrintToString();

            actual.Should().Be("Int32[]\n1\r\n2\r\n3\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintElements_WhenCalledOnList()
        {
            var list = new List<string> {"a", "b", "c"};

            var actual = list.PrintToString();

            actual.Should().Be("List<String>\na\r\nb\r\nc\r\n");
        }


        [Test]
        public void PrintToString_ShouldPrintKeyValuePairs_WhenCalledOnDictionary()
        {
            var dictionary = new Dictionary<string, int>()
            {
                ["a"] = 1,
                ["b"] = 2
            };

            var actual = dictionary.PrintToString();

            actual.Should().Be("Dictionary<String, Int32>\n" +
                               "KeyValuePair<String, Int32>\n" +
                               "\t\tKey = a\r\n\t\tValue = 1\r\n" +
                               "KeyValuePair<String, Int32>\n" +
                               "\t\tKey = b\r\n\t\tValue = 2\r\n");
        }

        [Test]
        public void PrintToString_ShouldPrintObjectOnce_WhenObjectHaveCyclicReferences()
        {
            var firstPerson = new Person(2, "Bob", 190, 20, null);
            var secondPerson = new Person(3, "Alice", 170, 19, null);
            firstPerson.Friends = new[] {secondPerson};
            secondPerson.Friends = new[] {firstPerson};

            var actual = firstPerson.PrintToString();

            actual.Should().Be("Person\n\tId = 2\r\n\tName = Bob" +
                               "\r\n\tHeight = 190\r\n\tAge = 20\r\n\tFriends = Person[]\n" +
                               "\t\tPerson\n\t\t\tId = 3\r\n\t\t\tName = Alice" +
                               "\r\n\t\t\tHeight = 170\r\n\t\t\tAge = 19" +
                               "\r\n\t\t\tFriends = Person[]\n\t\t\t\tCyclic reference \r\n");
        }
    }
}