using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;
using ObjectPrintingTests.TestingSource;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterShould
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            person = new Person
            {
                Name = "Thomas Anderson",
                Age = 119,
                Height = 180.4,
                Id = Guid.NewGuid()
            };
        }

        private Person person;

        [Test]
        public void AcceptanceTest()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(i => i.ToString("X"))
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name).TrimmedToLength(6)
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);
            var s2 = person.PrintToString();
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void ExcludeMember_WhenItsTypeExcluded()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();

            var result = printer.PrintToString(person);

            result.Should().NotContain("Id = ");
        }

        [Test]
        public void UseAlternativeTypeSerializer_WhenItAppointed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => i.ToString("X"));

            var result = printer.PrintToString(person);

            result.Should().Contain($"Age = {person.Age:X}");
        }

        [Test]
        public void UseAlternativeCulture_WhenItAppointed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture);

            var result = printer.PrintToString(person);

            result.Should().Contain($"Height = {person.Height.ToString(CultureInfo.InvariantCulture)}");
        }

        [Test]
        public void UseAlternativeMemberSerializer_WhenItAppointed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(i => i.ToString("X"));

            var result = printer.PrintToString(person);

            result.Should().Contain($"Age = {person.Age:X}");
        }

        [Test]
        public void TrimStrings_WhenTrimAppointed()
        {
            var length = 6;
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(length);

            var result = printer.PrintToString(person);

            result.Should().Contain($"Name = {person.Name[..length]}");
        }

        [Test]
        public void ExcludeMember_WhenMemberExcludingAppointed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);

            var result = printer.PrintToString(person);

            result.Should().NotContain($"Age = {person.Age}");
        }

        [Test]
        public void NotifyAboutCyclicReference()
        {
            var person = new Child
            {
                Name = "Thomas Anderson",
                Age = 119,
                Height = 180.4,
                Id = Guid.NewGuid()
            };
            person.Parent = person;

            var result = person.PrintToString();

            result.ToLower().Should().Contain("cyclic reference");
        }

        [Test]
        public void NotThrowStackOverflow_OnCyclicReference()
        {
            var person = new Child
            {
                Name = "Thomas Anderson",
                Age = 119,
                Height = 180.4,
                Id = Guid.NewGuid()
            };
            person.Parent = person;

            Action act = () => person.PrintToString();

            act.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void SerializeLists()
        {
            var dayOfWeeks = new List<string>
            {
                "Понедельник",
                "Вторник",
                "Среда",
                "Четверг",
                "Пятница",
                "Суббота",
                "Воскресенье"
            };

            var result = dayOfWeeks.PrintToString();
            Console.WriteLine(result);

            result.Should().Contain(string.Join(Environment.NewLine, dayOfWeeks));
        }
        
        [Test]
        public void SerializeArrays()
        {
            var dayOfWeeks = new string[]
            {
                "Понедельник",
                "Вторник",
                "Среда",
                "Четверг",
                "Пятница",
                "Суббота",
                "Воскресенье"
            };

            var result = dayOfWeeks.PrintToString();
            Console.WriteLine(result);

            result.Should().Contain(string.Join(Environment.NewLine, dayOfWeeks));
        }
        
        [Test]
        public void SerializeDictionarys()
        {
            var dict = new Dictionary<int, string>
            {
                {1, "один"},
                {2, "два"},
                {3, "три"},
                {4, "четыре"}
            };

            var result = dict.PrintToString();
            Console.WriteLine(result);

            result.Should().Contain(string.Join(Environment.NewLine,
                dict.Select(p => $"{p.Key} : {p.Value}")));
        }
    }
}