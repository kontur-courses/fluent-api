using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Solved.Tests;

namespace ObjectPrintingUnitTest
{
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Id = Guid.NewGuid(), Name = "Maxim", Age = 21, Height = 180.2 };
        }

        [Test]
        public void PrintToString_ShouldExcludedType()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();

            printer.PrintToString(person).Should().Be("Person\r\n\tName = Maxim\r\n\tHeight = 180,2\r\n\tAge = 21\r\n\tAnotherPerson = null\r\n");
        }

        [Test]
        public void PrintToString_ShouldExcludedMember()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Id);

            printer.PrintToString(person).Should().Be("Person\r\n\tName = Maxim\r\n\tHeight = 180,2\r\n\tAge = 21\r\n\tAnotherPerson = null\r\n");
        }

        [Test]
        public void PrintToString_AlternativeMethodSerialization_Double_ShouldBeNumberWithAAA()
        {
            var printer = ObjectPrinter.For<double>().Printing<double>().Using(i => i + "AAA");

            printer.PrintToString(1.9992929292).Should().Be("1,9992929292AAA\r\n");
        }

        [Test]
        public void PrintToString_AlternativeMethodSerialization_Member_ShouldBeModified()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(i => i + " years old");

            printer.PrintToString(person).Should().Be("Person\r\n\tId = Guid\r\n\tName = Maxim\r\n\tHeight = 180,2\r\n\tAge = 21 years old\r\n\tAnotherPerson = null\r\n");
        }

        [Test]
        public void PrintToString_SetCulture_ShouldBe_DoubleWithDot()
        {
            ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString(person).Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = Maxim\r\n\tHeight = 180.2\r\n\tAge = 21\r\n\tAnotherPerson = null\r\n");
        }

        [Test]
        public void PrintToString_TrimString_ShouldBe_StringLenghtThree()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(3);

            printer.PrintToString(person).Should().Be("Person\r\n\tId = Guid\r\n\tName = Max\r\n\tHeight = 180,2\r\n\tAge = 21\r\n\tAnotherPerson = null\r\n");
        }

        [Test]
        public void PrintToString_ShouldNotThrow_CyclicReference()
        {
            var anotherPerson = new Person
            {
                Name = "Maxim",
                Age = 21,
                Height = 180.2,
                AnotherPerson = person,
            };

            person.AnotherPerson = anotherPerson;

            Action act = () => Console.WriteLine(ObjectPrinter.For<Person>().PrintToString(person));

            act.Should().NotThrow();
        }

        [Test]
        public void PrintToString_ShouldBeCorrect_IfCollection()
        {
            var persons = new List<Person>();

            var anotherPerson = new Person
            {
                Name = "Maxim",
                Age = 21,
                Height = 180.2,
                AnotherPerson = person,
            };

            persons.Add(person);

            persons.Add(anotherPerson);

            ObjectPrinter.For<List<Person>>()
                         .PrintToString(persons)
                         .Should()
                         .Be("\tList`1\r\n\t0 Person\r\n\t\tId = Guid\r\n\t\tName = Maxim\r\n\t\tHeight = 180,2\r\n\t\tAge = 21\r\n\t\tAnotherPerson = null\r\n\t1 Person\r\n\t\tId = Guid\r\n\t\tName = Maxim\r\n\t\tHeight = 180,2\r\n\t\tAge = 21\r\n\t\tAnotherPerson = circular references\r\n");
        }

        [Test]
        public void PrintToString_ShouldBeCorrect_IfDictonary()
        {
            var persons = new Dictionary<int,Person>();

            var anotherPerson = new Person
            {
                Name = "Maxim",
                Age = 21,
                Height = 180.2,
                AnotherPerson = person,
            };

            persons.Add(1, person);

            persons.Add(2, anotherPerson);

            ObjectPrinter.For<Dictionary<int,Person>>().PrintToString(persons).Should().Be("\tDictionary`2\r\n\t0 KeyValuePair`2\r\n\t\tKey = 1\r\n\t\tValue = Person\r\n\t\t\tId = Guid\r\n\t\t\tName = Maxim\r\n\t\t\tHeight = 180,2\r\n\t\t\tAge = 21\r\n\t\t\tAnotherPerson = null\r\n\t1 KeyValuePair`2\r\n\t\tKey = 2\r\n\t\tValue = Person\r\n\t\t\tId = Guid\r\n\t\t\tName = Maxim\r\n\t\t\tHeight = 180,2\r\n\t\t\tAge = 21\r\n\t\t\tAnotherPerson = circular references\r\n");
        }
    }
}
