using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Should_BeAbleTo_ExcludeTypes()
        {
            var person = new Person {Name = "Test", Age = 10};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Excluding<DateTime>()
                .Excluding<double>()
                .Excluding<string>()
                .Excluding(x => x.Field);

            printer.PrintToString(person)
                .Should().Be("Person\r\n\tAge = 10\r\n");
        }

        [Test]
        public void Should_BeAbleTo_SerializeTypesAlternatively()
        {
            var person = new Person { Name = "Test", Age = 10 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding<int>()
                .Excluding<DateTime>()
                .Printing<string>().Using(x => x.Length.ToString());

            printer.PrintToString(person)
                .Should().Be("Person\r\n\tName = 4\r\n\tLastName = null\r\n");
        }

        [Test]
        public void Should_BeAbleTo_SetCultureForTypes()
        {
            var person = new Person { Name = "Test", DateOfBirth = new DateTime(2004, 02, 17)};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding<int>()
                .Printing<DateTime>().UsingCulture(CultureInfo.InvariantCulture);

            printer.PrintToString(person)
                .Should().Be($"Person\r\n\tName = {person.Name}\r\n\tLastName = null\r\n\tDateOfBirth = 02/17/2004 00:00:00\r\n");
        }

        [Test]
        public void Should_BeAbleTo_SerializeSpecialPropertyAlternatively()
        {
            var person = new Person { Name = "Test", LastName = "Test", Age = 10 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Excluding<int>()
                .Excluding<double>()
                .Excluding<DateTime>()
                .Printing(x => x.Name).Using(x => x.ToUpper());

            printer.PrintToString(person)
                .Should().Be($"Person\r\n\tName = {person.Name.ToUpper()}\r\n\tLastName = {person.LastName}\r\n");
        }

        [Test]
        public void Should_BeAbleTo_TrimStringMembers()
        {
            var person = new Person { Name = "TestToTrim", LastName = "TestToTrim", Age = 10 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Excluding<int>()
                .Excluding<double>()
                .Excluding<DateTime>()
                .Printing<string>().TrimmedToLength(4);

            printer.PrintToString(person)
                .Should().Be("Person\r\n\tName = Test\r\n\tLastName = Test\r\n");
        }

        [Test]
        public void Should_BeAbleTo_ExcludeSpecialMember()
        {
            var person = new Person { Name = "Test", LastName = "Test", Age = 10, Height = 140.37};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<DateTime>()
                .Excluding<Guid>()
                .Excluding(x => x.LastName)
                .Excluding(x => x.Field);

            printer.PrintToString(person)
                .Should().Be(
                    $"Person\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}\r\n\tAge = {person.Age}\r\n");

        }

        [Test]
        public void Should_SupportFields()
        {
            var person = new Person { Name = "Test", LastName = "Test", Age = 10, Height = 140.37 };
            person.Field = 2304;

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Excluding<DateTime>()
                .Printing(x => x.Field).Using(x => x.ToString("X"));

            printer.PrintToString(person)
                .Should().Be(
                    $"Person\r\n\tName = Test\r\n\tLastName = Test\r\n\tHeight = 140,37\r\n\tAge = 10\r\n\tField = {person.Field:X}\r\n");
        }

        [Test]
        public void Should_SupportArray()
        {
            var arr = new[] {1, 2, 3, 4, 5};
            var printer = ObjectPrinter.For<int[]>();
            Console.WriteLine(printer.PrintToString(arr));
        }

        [Test]
        public void Should_SupportList()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var printer = ObjectPrinter.For<List<int>>();
            Console.WriteLine(printer.PrintToString(list));
        }

        [Test]
        public void Should_SupportDictionary()
        {
            var dict = new Dictionary<int, string> {{1, "One"}, {2, "Two"}, {3, "Three"}, {4, "Four"}, {5, "Five"}};
            var printer = ObjectPrinter.For<Dictionary<int, string>>();
            Console.WriteLine(printer.PrintToString(dict));
        }
    }
}