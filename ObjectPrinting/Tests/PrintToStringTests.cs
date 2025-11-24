using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintToStringTests
    {

        private class Node
        {
            public string Name { get; set; }
            public Node Other { get; set; }
        }

        private class Container
        {
            public int[] Numbers { get; set; }
            public List<string> List { get; set; }
            public Dictionary<string, int> Map { get; set; }
        }

        [Test]
        public void PrintToString_NullObject_PrintsNull()
        {
            var printer = ObjectPrinter.For<Person>();
            string result = printer.PrintToString(null);

            result.Should().Be("null" + Environment.NewLine);
        }

        [Test]
        public void PrintToString_SimpleObject_ContainsTypeAndMembers()
        {
            var person = new Person { Id = Guid.NewGuid(), Name = "Петя", Height = 1.82, Age = 19 };
            string s = ObjectPrinter.For<Person>().PrintToString(person);

            s.Should().Contain(nameof(Person));
            s.Should().Contain(nameof(Person.Name));
            s.Should().Contain(nameof(Person.Age));
            s.Should().Contain(nameof(Person.Height));
            s.Should().Contain(person.Name);
            s.Should().Contain(person.Age.ToString());
        }

        [Test]
        public void PrintToString_ExcludingType_DoesNotContainValueOfThatType()
        {
            var person = new Person { Id = Guid.NewGuid(), Name = "Петя", Height = 1.75, Age = 30 };
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();

            string s = printer.PrintToString(person);

            s.Should().NotContain(person.Id.ToString());
        }

        [Test]
        public void PrintToString_TypeSerializer_AppliesSerializerForType()
        {
            var person = new Person { Name = "X", Age = 255 };
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => i.ToString("X"));

            string s = printer.PrintToString(person);

            s.Should().Contain(person.Age.ToString("X"));
        }

        [Test]
        public void PrintToString_TypeCulture_AppliesCultureForIFormattable()
        {
            var person = new Person { Name = "Y", Height = 1234.56 };
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture);

            string s = printer.PrintToString(person);

            var expected = person.Height.ToString(CultureInfo.InvariantCulture);
            s.Should().Contain(expected);
        }

        [Test]
        public void PrintToString_MemberSerializer_AppliesToSpecificMember()
        {
            var person = new Person { Name = "Петя", Age = 20 };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).Using(n => $"<{n}>");

            string s = printer.PrintToString(person);

            s.Should().Contain($"<{person.Name}>");
        }

        [Test]
        public void PrintToString_TrimmedToLength_TruncatesStringMember()
        {
            var person = new Person { Name = "Василий", Age = 40 };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(3);

            string s = printer.PrintToString(person);

            s.Should().Contain(person.Name.Substring(0, 3));
            s.Should().NotContain(person.Name.Substring(0, 4));
        }

        [Test]
        public void PrintToString_ExcludingMember_DoesNotContainMemberValue()
        {
            var person = new Person { Name = "Петя", Age = 77 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);

            string s = printer.PrintToString(person);

            s.Should().NotContain(person.Age.ToString());
        }

        [Test]
        public void PrintToString_CircularReferences_DoesNotStackOverflowAndShowsMarker()
        {
            var a = new Node { Name = "A" };
            var b = new Node { Name = "B" };
            a.Other = b;
            b.Other = a;

            string s = ObjectPrinter.For<Node>().PrintToString(a);

            s.Should().Contain(a.Name);
            s.Should().Contain(b.Name);
            s.Should().MatchRegex("(?i).*Циклическая.*");
        }

        [Test]
        public void PrintToString_Collections_ArraysListsAndDictionariesAreSerialized()
        {
            var container = new Container
            {
                Numbers = new[] { 1, 2 },
                List = new List<string> { "x", "y" },
                Map = new Dictionary<string, int> { { "k", 42 } }
            };

            string s = ObjectPrinter.For<Container>().PrintToString(container);

            s.Should().Contain(nameof(Container));
            s.Should().Contain(nameof(Container.Numbers));
            s.Should().Contain("[0]");
            s.Should().Contain("1");
            s.Should().Contain(nameof(Container.List));
            s.Should().Contain("x");
            s.Should().Contain(nameof(Container.Map));
            s.Should().Contain("Key");
            s.Should().Contain("Value");
            s.Should().Contain("k");
            s.Should().Contain("42");
        }
    }
}
