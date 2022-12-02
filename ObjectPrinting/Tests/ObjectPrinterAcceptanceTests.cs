using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [SetUp]
        public void InitPerson()
        {
            _person = new Person { Name = "Alex", Age = 19, Height = 165.5 };
        }

        private Person _person;

        [Test]
        public void ObjectPrinter_WhenExcludingType_SerializesWithoutExcluded()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();

            var expected =
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 165,5\r\n\tFather = null\r\n\tSon = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_WithSpecialSerialization_WorksCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .With<int>(_ => "Num serialized");

            var expected =
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 165,5\r\n\tAge = Num serialized\n\tFather = null\r\n\tSon = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_WithMemberSpecialSerialization_WorksCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .ForMember(p => p.Age)
                .SetSerialization(age => (age + 10).ToString());
            var expected =
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 165,5\r\n\tAge = 29\n\tFather = null\r\n\tSon = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_WhenExcludingProperty_WorksCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Height);

            var expected =
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tAge = 19\r\n\tFather = null\r\n\tSon = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_WhenHasRecursion_WorksCorrectly()
        {
            var father = new Person { Name = "Jack", Age = 45 };
            _person.Father = father;

            var printer = ObjectPrinter.For<Person>()
                .Excluding<double>()
                .Excluding(p => p.Height)
                .ForMember(p => p.Age).SetSerialization(age => (age + 42).ToString());
            var expected =
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tAge = 61\n\tFather = Person\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\tName = Jack\r\n\t\tAge = 87\n\t\tFather = null\r\n\t\tSon = null\r\n\tSon = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_WhenCultureSet_WorksCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .With<double>().SetCulture(new CultureInfo("en"));


            var expected =
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 165.5\n\tAge = 19\r\n\tFather = null\r\n\tSon = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_SetMaxStringLength_WorksCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .With<string>().SetMaxStringLength(1);

            var expected =
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = A\n\tHeight = 165,5\r\n\tAge = 19\r\n\tFather = null\r\n\tSon = null\r\n";
            var a = printer.PrintToString(_person);
            printer.PrintToString(_person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_WhenObjectHasCyclicReferences_notBeStackOverflow()
        {
            var father = new Person { Name = "Jack", Age = 45, Son = _person };
            _person.Father = father;
            var printer = ObjectPrinter.For<Person>();
            var print = new Action(() => { printer.PrintToString(_person); });
            print.Should().NotThrow<StackOverflowException>();
        }
    }
}