using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class ObjectPrinting_Should
    {
        private PrintingConfig<Person> printer;
        private Person person;
        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person
            {
                Id = Guid.Empty,
                Name = "John Smith",
                Age = 69,
                Height = 13.37
            };
        }

        [Test]
        public void ObjectPrinter_ShouldExcludeTypeFromSerialization()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = John Smith", "\tHeight = 13,37")
                           + Environment.NewLine;

            printer.Excluding<int>();

            printer.PrintToString(person).Should().Be(expected);
        }
        
        [Test]
        public void ObjectPrinter_ShouldSerializeTypesAlternatively()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = John Smith", 
                               "\tHeight = 13,37", "\tAge = 69 (это инт)")
                           + Environment.NewLine;

            printer.Printing<int>().Using(e => e + " (это инт)");

            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldSetCulture()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = John Smith",
                               "\tHeight = 13.37", "\tAge = 69")
                           + Environment.NewLine;

            printer.Printing<double>().Using(CultureInfo.InvariantCulture);

            printer.PrintToString(person).Should().Be(expected);
        }


        [Test]
        public void ObjectPrinter_ShouldSerializePropertiesAlternatively()
        {
            var expected = string.Join(Environment.NewLine, "PersonExtended", "\tMiddleName = Johnovich", "\tId = Guid", 
                               "\tName = John Smith (это имя)",
                               "\tHeight = 13,37", "\tAge = 69")
                           + Environment.NewLine;

            var printer = ObjectPrinter.For<PersonExtended>();
            var person = new PersonExtended
            {
                Id = Guid.Empty,
                Name = "John Smith",
                MiddleName = "Johnovich",
                Age = 69,
                Height = 13.37
            };

            printer.Printing(e => e.Name).Using(e => e + " (это имя)");

            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldTrimStringProperties()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = John",
                               "\tHeight = 13,37", "\tAge = 69")
                           + Environment.NewLine;

            printer.Printing(e => e.Name).TrimmedToLength(4);

            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldExcludePropertyFromSerialization()
        {
            var expected = string.Join(Environment.NewLine, "Person", "\tName = John Smith", "\tHeight = 13,37", "\tAge = 69")
                           + Environment.NewLine;

            printer.Excluding(e => e.Id);

            printer.PrintToString(person).Should().Be(expected);
       }


        [Test]
        public void ObjectPrinter_ShouldOverrideAlternativeTypeSerialization_IfPropertySerializationSpecified()
        {
            var expected = string.Join(Environment.NewLine, "PersonExtended", "\tMiddleName = Johnovich (это строка)", "\tId = Guid",
                               "\tName = John Smith (это имя (форматирование для типа не применилось))",
                               "\tHeight = 13,37", "\tAge = 69")
                           + Environment.NewLine;

            var printer = ObjectPrinter.For<PersonExtended>();
            var person = new PersonExtended
            {
                Id = Guid.Empty,
                Name = "John Smith",
                MiddleName = "Johnovich",
                Age = 69,
                Height = 13.37
            };

            printer
                .Printing(e => e.Name).Using(e => e + " (это имя (форматирование для типа не применилось))")
                .Printing<string>().Using(e => e + " (это строка)");

            printer.PrintToString(person).Should().Be(expected);
        }
    }
}
