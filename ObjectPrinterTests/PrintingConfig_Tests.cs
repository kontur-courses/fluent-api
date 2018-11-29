using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrinterTests
{
    [TestFixture]
    public class PrintingConfig_Tests
    {
        private PrintingConfig<Person> printer;
        private Person person;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person {Name = "Alex", Age = 19, Height = 183};
        }

        [Test]
        public void Printing_NoExludedFields_WhenExludeConfigured()
        {
            printer.Excluding<Guid>();
            string stringifyPerson = printer.PrintToString(person);
            string expected = "{ObjectPrinting.Tests.Person}\r\n" +
                              "Name: \"Alex\"\r\n" +
                              "Height: 183\r\n" +
                              "Age: 19\r\n";
            stringifyPerson.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Printing_CustomTypeSerializationWorks_WhenConfigured()
        {
            printer.Printing<int>().Using(i => "cistomizedForInt32");
            string expected =
                "{ObjectPrinting.Tests.Person}\r\n" +
                "Id: 00000000-0000-0000-0000-000000000000\r\n" +
                "Name: \"Alex\"\r\n" +
                "Height: 183\r\n" +
                "Age: \"cistomizedForInt32\"\r\n";
            var stringifyPerson = printer.PrintToString(person);
            stringifyPerson.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Printing_CultureSetted_Whenconfigured()
        {
            printer.Printing<double>().Using(CultureInfo.InvariantCulture);
            var expected =
                "{ObjectPrinting.Tests.Person}\r\n" +
                "Id: 00000000-0000-0000-0000-000000000000\r\n" +
                "Name: \"Alex\"\r\n" +
                "Height: \"183 - Инвариантный язык (Инвариантная страна)\"\r\n" +
                "Age: 19\r\n";
            var stringifyPerson = printer.PrintToString(person);
            stringifyPerson.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Printing_PropertyParsedUncommon_WhenHasCustomFunc()
        {
            printer.Printing(p => p.Age).Using(i => "ageParsingIsCustomized");
            var expected =
                "{ObjectPrinting.Tests.Person}\r\n" +
                "Id: 00000000-0000-0000-0000-000000000000\r\n" +
                "Name: \"Alex\"\r\n" +
                "Height: 183\r\n" +
                "Age: \"ageParsingIsCustomized\"\r\n";
            var stringifyPerson = printer.PrintToString(person);
            stringifyPerson.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Printing_StringFielsAreTrimmed_WhenTrimmingWasSet()
        {
            printer.Printing(p => p.Name).TrimmedToLength(2);
            var expected =
                "{ObjectPrinting.Tests.Person}\r\n" +
                "Id: 00000000-0000-0000-0000-000000000000\r\n" +
                "Name: \"Al\"\r\n" +
                "Height: 183\r\n" +
                "Age: 19\r\n";
            var stringifyPerson = printer.PrintToString(person);
            stringifyPerson.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Printing_HasNoExludedFields_WhenFieldsToExcludedWasAdded()
        {
            printer.ExcludingProperty(p => p.Age);
            var expected =
                "{ObjectPrinting.Tests.Person}\r\n" +
                "Id: 00000000-0000-0000-0000-000000000000\r\n" +
                "Name: \"Alex\"\r\n" +
                "Height: 183\r\n";
            var stringifyPerson = printer.PrintToString(person);
            stringifyPerson.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Printing_CollectionParsed()
        {
            var alex = new Person {Name = "Alex", Age = 23, Height = 169};
            var george = new Person {Name = "George", Age = 19, Height = 177};
            var andrew = new Person {Name = "Andrew", Age = 54, Height = 166};
            var viola = new Person {Name = "Viola", Age = 13, Height = 167};
            
            var personList = new List<Person>()
            {
                alex,
                george,
                andrew,
                viola
            };

            printer.ExcludingProperty(p => p.Age);
            var stringifyList = personList.PrintToString();

            var expected =
                "{System.Collections.Generic.List`1[[ObjectPrinting.Tests.Person, ObjectPrinting, " +
                "Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]}\r\n";
            expected += alex.PrintToString();
            expected += george.PrintToString();
            expected += andrew.PrintToString();
            expected += viola.PrintToString();
            stringifyList.Should().BeEquivalentTo(expected);
        }
    }
}