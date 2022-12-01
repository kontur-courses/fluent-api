using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class PrintingConfigTests
    {
        private readonly Person person = new Person { Name = "Alex", Age = 19 };
        private PrintingConfig<Person> printingConfig;

        [SetUp]
        public void CreateObjectPrinter()
        {
            printingConfig = ObjectPrinter.For<Person>();
        }

        [Test]
        public void Should_CreatePrintingConfig_WhenCtorHasNullArgument()
        {
            Action act = () => new PrintingConfig<Person>();

            act.Should().NotThrow("valid constructor");
        }

        [Test]
        public void Should_NotPrint_PrivateMembersOfObject()
        {
            printingConfig.PrintToString(person).Should().
                NotContain("secretNumber").
                And.NotContain("SecretNumber");
        }

        [Test]
        public void Should_ExcludingPropertyByName()
        {
            CheckThatNotContainIn(printingConfig.Excluding(p => p.Name), nameof(Person.Name));
        }

        private void CheckThatNotContainIn(PrintingConfig<Person> printer, string propertyNames)
        {
            var printedObject = printer.PrintToString(person);
            printedObject.Should().NotContain(propertyNames);
        }

        [Test]
        public void Should_ExcludingPropertyByType()
        {
            CheckThatNotContainIn(printingConfig.Excluding<string>(), nameof(Person.Name));
            CheckThatNotContainIn(printingConfig.Excluding<Guid>(), nameof(Person.Id));
            CheckThatNotContainIn(printingConfig.Excluding<decimal>(), nameof(Person.Height));
            CheckThatNotContainIn(printingConfig.Excluding<int>(), nameof(Person.Age));
        }

        [Test]
        public void Should_ExcludingPropertyByType_ThatNotInherited()
        {
            CheckThatNotContainIn(printingConfig.Excluding<string>(), nameof(Person.Name));
            var printedObject = printingConfig.PrintToString(person);
            printedObject.Should().Contain(nameof(Person.Name));
        }

        [Test]
        public void Should_PrintedOnce_LoopedReferencedMembers()
        {
            person.BestFriend = new Person() { Name = "Igor", BestFriend = person };

            var printedObject = printingConfig.PrintToString(person);

            printedObject.Should().Contain(person.Name, Exactly.Once()).And.Contain("recursive reference", Exactly.Once());
        }

        [Test]
        public void Should_PrintToString_ListOfPerson()
        {
            var persons = new List<Person>
            {
                person,
                new Person() { Name = "Igor", Age = 17 }
            };
            var printedObject = ObjectPrinter.For<List<Person>>().PrintToString(persons);
            printedObject.Should().ContainAll(persons[0].Name, persons[1].Name);
        }

        [Test]
        public void Should_PrintToString_ArrayOfPerson()
        {
            var persons = new [] {person, new Person() { Name = "Igor", Age = 17} };
            var printedObject = ObjectPrinter.For<Person[]>().PrintToString(persons);
            printedObject.Should().ContainAll(persons[0].Name, persons[1].Name);
        }

        [Test]
        public void Should_PrintToString_DictionaryOfPerson()
        {
            var persons = new Dictionary<int, Person>
            {
                { 1, person },
                { 2, new Person() { Name = "Igor", Age = 17 } }
            };
            var printedObject = ObjectPrinter.For<Dictionary<int, Person>>().PrintToString(persons);
            printedObject.Should().ContainAll(persons[1].Name, persons[2].Name);
        }
    }
}
