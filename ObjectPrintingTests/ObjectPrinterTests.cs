using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            person = new Person
            {
                Age = 19,
                Birthdate = new DateTime(2001, 12, 20),
                Name = "Sergey",
                Surname = "Tretyakov",
                Father = new Person { Name = "Dad", Height = 178 },
                Height = 193.5,
                Id = new Guid()
            };
        }

        [Test]
        public void Should_ExcludeType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();

            printer.PrintToString(person)
                .Should().NotContain(person.Name)
                .And.NotContain(person.Surname);
        }

        [Test]
        public void Should_ExcludeMember()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Name);

            printer.PrintToString(person)
                .Should().NotContain(person.Name)
                .And.Contain(person.Surname);
        }

        [Test]
        public void Should_UseTypeSerializer()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(x => x.Length.ToString());

            printer.PrintToString(person)
                .Should().Contain($"{nameof(person.Name)} = {person.Name.Length}")
                .And.Contain($"{nameof(person.Surname)} = {person.Surname.Length}");
        }

        [Test]
        public void Should_UseMemberSerializer()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).Using(x => x.ToUpper());

            printer.PrintToString(person)
                .Should().Contain($"{nameof(person.Name)} = {person.Name.ToUpper()}")
                .And.Contain($"{nameof(person.Surname)} = {person.Surname}");
        }

        [Test]
        public void Should_UseCulture()
        {
            var culture = CultureInfo.CreateSpecificCulture("fr-FR");
            var printer = ObjectPrinter.For<Person>()
                .Printing<DateTime>().Using(culture);

            printer.PrintToString(person)
                .Should().Contain(person.Birthdate.ToString(culture));
        }

        [Test]
        public void Should_TrimStringType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(3);

            printer.PrintToString(person)
                .Should().NotContain(person.Name)
                .And.Contain("Ser")
                .And.NotContain(person.Surname)
                .And.Contain("Tre");
        }

        [Test]
        public void Should_TrimMember()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(3);

            printer.PrintToString(person)
                .Should().NotContain(person.Name)
                .And.Contain("Ser")
                .And.Contain(person.Surname);
        }

        [Test]
        public void Should_MarkLoopReference()
        {
            person.Father = person;
            var printer = ObjectPrinter.For<Person>();

            printer.PrintToString(person)
                .Should().Contain("Loop reference");
        }

        [Test]
        public void Should_MarkNestedLoopReference()
        {
            person.Father.Father = person;
            var printer = ObjectPrinter.For<Person>();

            printer.PrintToString(person)
                .Should().Contain("Loop reference");
        }

        [Test]
        public void Should_MarkLoopReference_OnEachLevel()
        {
            person.Father.Father = person;
            person.Mother = person;
            var printer = ObjectPrinter.For<Person>();

            printer.PrintToString(person)
                .Should().Contain($"{nameof(person.Father)} = [Loop reference]")
                .And.Contain($"{nameof(person.Mother)} = [Loop reference]");
        }

        [Test]
        public void Should_SerializeArray()
        {
            var arr = new[] { 3, 2, 1 };
            var printer = ObjectPrinter.For<int[]>();

            printer.PrintToString(arr).Should()
                .ContainAll(arr.Select(x => x.ToString()));
        }

        [Test]
        public void Should_SerializeList()
        {
            var list = new List<int> { 3, 2, 1 };
            var printer = ObjectPrinter.For<List<int>>();

            printer.PrintToString(list).Should()
                .ContainAll(list.Select(x => x.ToString()));
        }

        [Test]
        public void Should_SerializeDictionary()
        {
            var dict = new Dictionary<int, string> { { 1, "3" }, { 2, "2" }, { 3, "1" } };
            var printer = ObjectPrinter.For<Dictionary<int, string>>();
            printer.PrintToString(dict).Should()
                .ContainAll(dict.Keys.Select(x => x.ToString()))
                .And.ContainAll(dict.Values.Select(x => x.ToString()));
        }

        [Test]
        public void Should_SerializeEmptyCollection()
        {
            var list = new List<int>();
            var printer = ObjectPrinter.For<List<int>>();
            printer.PrintToString(list)
                .Should().Contain("Empty");
        }
    }
}