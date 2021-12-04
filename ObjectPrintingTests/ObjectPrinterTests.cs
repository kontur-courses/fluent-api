using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Common;
using ObjectPrinting.Extensions;
using ObjectPrintingTests.Common;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private (string Expected, string Actual, bool IsUnexpected) testData;

        [SetUp]
        public void SetUp()
        {
            person = new Person()
            {
                FullName = new FullName()
                {
                    FirstName = "Max",
                    LastName = "Payne"
                },
                Age = 19,
                Height = 175.50
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (string.IsNullOrEmpty(testData.Expected) && string.IsNullOrEmpty(testData.Actual))
                return;

            Console.WriteLine(testData.IsUnexpected
                ? $"Unexpected:\r\n\t{testData.Expected}\r\n"
                : $"Expected:\r\n\t{testData.Expected}\r\n");
            Console.WriteLine($"Actual:\r\n{testData.Actual}");
        }

        [Test]
        public void ObjectPrinter_ShouldExcludeType()
        {
            testData.IsUnexpected = true;
            testData.Expected = "Id = 00000000-0000-0000-0000-000000000000";

            testData.Actual = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .PrintToString(person);

            testData.Actual.Should().NotContain(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_ShouldExcludeProp()
        {
            testData.IsUnexpected = true;
            testData.Expected = "Id = 00000000-0000-0000-0000-000000000000";

            testData.Actual = ObjectPrinter.For<Person>()
                .Exclude(x => x.Id)
                .PrintToString(person);

            testData.Actual.Should().NotContain(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_ShouldProvideCustomTypeSerialization()
        {
            testData.IsUnexpected = false;
            testData.Expected = "Height = 175,50";

            testData.Actual = ObjectPrinter.For<Person>()
                .Serialize<double>()
                .Using(x => x.ToString("F"))
                .PrintToString(person);

            testData.Actual.Should().Contain(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_ShouldProvideCustomPropSerialization()
        {
            testData.IsUnexpected = false;
            testData.Expected = "175,50 cm";

            testData.Actual = ObjectPrinter.For<Person>()
                .Serialize(prop => prop.Height)
                .Using(x => $"{x:F} cm")
                .PrintToString(person);

            testData.Actual.Should().Contain(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_ShouldProvideCultureForFormattableTypes()
        {
            testData.IsUnexpected = false;
            testData.Expected = "Height = 175.5";

            testData.Actual = ObjectPrinter.For<Person>()
                .Serialize<double>()
                .Using(CultureInfo.InvariantCulture)
                .PrintToString(person);

            testData.Actual.Should().Contain(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_ShouldProvideTrimmingForStrings()
        {
            testData.IsUnexpected = false;
            testData.Expected = "LastName = Pay";

            testData.Actual = ObjectPrinter.For<Person>()
                .Serialize<string>()
                .Trim(3)
                .PrintToString(person);

            testData.Actual.Should().Contain(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_ShouldProvideObjectExtensions()
        {
            testData.IsUnexpected = false;
            testData.Expected = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .PrintToString(person);

            testData.Actual = person
                .PrintToString(x => x.Exclude<Guid>());

            testData.Actual.Should().Be(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_ShouldCatchReferenceCycles()
        {
            testData.IsUnexpected = false;
            testData.Expected = "Reference cycle detected! This object will be skipped";

            person.Parents = new[] {person};
            testData.Actual = person
                .PrintToString(x => x.Exclude<Guid>());

            testData.Actual.Should().Contain(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_ShouldPrintCollections()
        {
            testData.IsUnexpected = false;
            testData.Expected = "Parents = Person[]\r\n\t\tPerson\r\n\t\t\tFullName = null";
            person.Parents = new[]
            {
                new Person {Height = 175, Age = 30},
                new Person {Height = 200, Age = 27},
                new Person {Height = 183, Age = 23},
                new Person {Height = 165, Age = 16},
            };

            testData.Actual = person
                .PrintToString(x => x.Exclude<Guid>());

            testData.Actual.Should().Contain(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_ShouldPrintDictionaries()
        {
            testData.IsUnexpected = false;
            testData.Expected =
                "Documents = Dictionary`2\r\n\t\tPassport - 6511 403943\r\n\t\tDrive license - 11 214345345 45";
            person.Documents = new Dictionary<string, string>()
            {
                {"Passport", "6511 403943"},
                {"Drive license", "11 214345345 45"}
            };

            testData.Actual = person
                .PrintToString(x => x.Exclude<Guid>());

            testData.Actual.Should().Contain(testData.Expected);
        }

        [Test]
        public void ObjectPrinter_AcceptanceTest()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .Serialize<double>().Using(i => i.ToString("F"))
                .Serialize<double>().Using(CultureInfo.InvariantCulture)
                .Serialize(p => p.Age)
                .Using(x => $"{x} years")
                .Serialize(p => p.FullName.FirstName)
                .Trim(2)
                .Exclude(p => p.FullName.LastName);

            var s1 = printer.PrintToString(person);
            var s2 = person.PrintToString();
            var s3 = person.PrintToString(s => s.Exclude(p => p.Id));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}