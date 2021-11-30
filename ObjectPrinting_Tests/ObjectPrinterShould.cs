using System;
using System.Collections.Generic;
using System.Globalization;
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

            printer.PrintToString(person).Should().NotContain("Id = ");
        }

        [Test]
        public void UseAlternativeTypeSerializer_WhenItAppointed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => i.ToString("X"));

            printer.PrintToString(person).Should().Contain($"Age = {person.Age:X}");
        }

        [Test]
        public void UseAlternativeCulture_WhenItAppointed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture);

            printer.PrintToString(person).Should()
                .Contain($"Height = {person.Height.ToString(CultureInfo.InvariantCulture)}");
        }

        [Test]
        public void UseAlternativeMemberSerializer_WhenItAppointed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(i => i.ToString("X"));

            printer.PrintToString(person).Should().Contain($"Age = {person.Age:X}");
        }

        [Test]
        public void TrimStrings_WhenTrimAppointed()
        {
            var length = 6;
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(length);

            printer.PrintToString(person).Should().Contain($"Name = {person.Name.Substring(0, length)}");
        }

        [Test]
        public void ExcludeMember_WhenMemberExcludingAppointed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);

            printer.PrintToString(person).Should().NotContain($"Age = {person.Age}");
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

            person.PrintToString().ToLower().Should().Contain("циклическая ссылка");
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
        public void SerializeDictionary()
        {
            var dict = new Dictionary<int, string>()
            {
                {1, "one"},
                {2, "two"},
                {3, "three"}
            };
            var s1 = dict.PrintToString();
            Console.WriteLine(s1);
        }
    }
}