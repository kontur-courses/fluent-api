using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinterTests.ForSerialization;
using ObjectPrinting.Core;
using FluentAssertions;
using ObjectPrinting.Extensions;

namespace ObjectPrinterTests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private PrintingConfig<Person> _personPrinter;

        [SetUp]
        public void Setup()
        {
            _personPrinter = ObjectPrinter.For<Person>();
        }

        [Test]
        public void Excluding_ThrowException_WhenNotMemberExpression()
        {
            var act = new Action(() => _personPrinter.Excluding(person => person.Age + 1));

            act.Should().Throw<Exception>();
        }

        [Test]
        public void Excluding_NotException_WhenMemberExpression()
        {
            var act = new Action(() => _personPrinter.Excluding(person => person.Age));

            act.Should().NotThrow<Exception>();
        }

        [Test]
        public void Printing_ThrowException_WhenNotMemberExpression()
        {
            var act = new Action(() => _personPrinter.Printing(person => person.Age + 1));

            act.Should().Throw<Exception>();
        }

        [Test]
        public void Printing_NotException_WhenMemberExpression()
        {
            var act = new Action(() => _personPrinter.Printing(person => person.Age));

            act.Should().NotThrow<Exception>();
        }

        [Test]
        public void PrintToString_SimpleSerializedText_WhenNotConfiguration()
        {
            var act = _personPrinter.PrintToString(Instances.Person);

            act.Should()
                .Be(
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Danil\r\n\tHeight = 160.5\r\n\tAge = 15\r\n\t" +
                    "Weight = 67.778\r\n\tBirthPlace = null\r\n");
        }

        [Test]
        public void PrintToString_StringWithoutCertainTypeMembers_WhenExcludingMembersType()
        {
            var act = _personPrinter
                .Excluding<double>()
                .PrintToString(Instances.Person);

            act.Should()
                .Be(
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Danil\r\n\tAge = 15\r\n\tBirthPlace = null\r\n");
        }

        [Test]
        public void
            PrintToString_StringWithAlternativeSerializationCertainMembers_WhenAlternativeSerializationByMembersType()
        {
            var act = _personPrinter
                .Printing<double>()
                .Using(val => ((int) Math.Ceiling(val)).ToString())
                .PrintToString(Instances.Person);

            act.Should()
                .Be(
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Danil\r\n\tHeight = 161\r\n\tAge = 15\r\n\tWeight = 68\r\n\t" +
                    "BirthPlace = null\r\n");
        }

        [Test]
        public void PrintToString_StringWithCertainCultureOfMember_WhenSpecifyCulture()
        {
            var act = _personPrinter
                .Printing<double>()
                .SpecifyCulture(CultureInfo.CreateSpecificCulture("fr-CA"))
                .PrintToString(Instances.Person);

            act.Should()
                .Be(
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Danil\r\n\tHeight = 160,5\r\n\tAge = 15\r\n\tWeight = 67,778" +
                    "\r\n\tBirthPlace = null\r\n");
        }

        [Test]
        public void
            PrintToString_StringWithAlternativeSerializationOfCertainMember_WhenAlternativeSerializationByName()
        {
            var act = _personPrinter
                .Printing(person => person.Age)
                .Using(age => age + " years old")
                .PrintToString(Instances.Person);

            act.Should()
                .Be(
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Danil\r\n\tHeight = 160.5\r\n\tAge = 15 years old\r\n\t" +
                    "Weight = 67.778\r\n\tBirthPlace = null\r\n");
        }

        [Test]
        public void PrintToString_StringWithTrimmedStringMembers_WhenStringMembersAreTruncated()
        {
            var act = _personPrinter
                .Printing<string>()
                .TrimmedToLength(3)
                .PrintToString(Instances.Person);

            act.Should()
                .Be(
                    "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Dan\r\n\tHeight = 160.5\r\n\tAge = 15\r\n\tWeight = 67.778" +
                    "\r\n\tBirthPlace = null\r\n");
        }

        [Test]
        public void TrimmedToLength_ThrowException_WhenMaxLengthOfCroppedStringNotPositive()
        {
            var act = new Action(() => _personPrinter
                .Printing<string>()
                .TrimmedToLength(0));

            act.Should().Throw<Exception>();
        }

        [Test]
        public void PrintToString_StringWithoutCertainMember_WhenExcludingMemberByName()
        {
            var act = _personPrinter
                .Excluding(person => person.Id)
                .PrintToString(Instances.Person);

            act.Should()
                .Be("Person\r\n\tName = Danil\r\n\tHeight = 160.5\r\n\tAge = 15\r\n\tWeight = 67.778" +
                    "\r\n\tBirthPlace = null\r\n");
        }

        [Test]
        public void PrintToString_SerializedString_WhenUsingObjectPrinterExtensionsWithConfiguration()
        {
            var act = Instances.Person
                .PrintToString(config => config
                    .Excluding<Guid>()
                    .Printing<int>()
                    .Using(val => val + "*")
                    .Printing<string>()
                    .TrimmedToLength(3)
                    .Printing(person => person.Weight)
                    .Using(weight => weight + " kilo"));

            act.Should()
                .Be("Person\r\n\tName = Dan\r\n\tHeight = 160.5\r\n\tAge = 15*\r\n\tWeight = 67.778 kilo\r\n\t" +
                    "BirthPlace = null\r\n");
        }

        [Test]
        public void
            PrintToString_StringWithAlternativeSerializationByName_WhenIntersectionOfAlternativeSerializationByTypeAndName()
        {
            var act = _personPrinter
                .Printing(person => person.Name)
                .Using(name => name + " years old")
                .Printing<string>()
                .Using(str => str + " ***")
                .PrintToString(Instances.Person);

            act.Should().Contain("Name = Danil years old");
        }
    }
}