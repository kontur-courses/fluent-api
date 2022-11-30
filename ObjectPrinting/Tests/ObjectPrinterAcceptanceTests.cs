using System;
using System.Globalization;
using System.Net.Mail;
using System.Security.Cryptography;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person _person;
        [SetUp]
        public void InitPerson()
        {
            _person = new Person { Name = "Alex", Age = 19 };
        }

        [Test]
        public void ObjectPrinter_WhenExcludingType_SerializesWithoutExcluded()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();
            
            var expected = $"Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tFather = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_WithSpecialSerialization_WorksCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .With<int>(_ => "Num serialized");

            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = Num serialized\tFather = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);

        }

        [Test]
        public void ObjectPrinter_WithMemberSpecialSerialization_WorksCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .ForMember(p => p.Age)
                .SetSerialization(age => (age + 10).ToString())
                .ApplyConfig();
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 29\tFather = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_WhenExcludingProperty_WorksCorrectly()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Height);
            
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tAge = 19\r\n\tFather = null\r\n";
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
                .ForMember(p => p.Age).SetSerialization(age => (age + 42).ToString())
                .ApplyConfig();
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tAge = 61\tFather = Person\r\n\t\tId = Guid\r\n\t\tName = Jack\r\n\t\tAge = 87\t\tFather = null\r\n";
            printer.PrintToString(_person).Should().Be(expected);
        }
    }
}