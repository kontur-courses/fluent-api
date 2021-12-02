using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Common;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
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
                Name = "Alex",
                Age = 19,
                Height = 175.50
            };
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine(testData.IsUnexpected
                ? $"Unexpected:\r\n\t{testData.Expected}\r\n"
                : $"Expected:\r\n\t{testData.Expected}\r\n");
            Console.WriteLine($"Actual:\r\n\t{testData.Actual}");
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
            testData.Expected = "Height - 175,50 cm";

            testData.Actual = ObjectPrinter.For<Person>()
                .Serialize(prop => prop.Height)
                .Using(x => $"Height - {x:F} cm")
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
            testData.Expected = "Name = Al";

            testData.Actual = ObjectPrinter.For<Person>()
                .Serialize<string>()
                .Trim(2)
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
    }
}