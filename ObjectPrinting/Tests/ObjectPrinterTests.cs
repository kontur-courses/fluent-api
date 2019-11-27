using System;
using System.Collections;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Age = 20, Height = 170.5, Id = new Guid(), Name = "Alex", Surname = "Johnson"};
        }


        [Test]
        public void Excluding_ShouldExcludeCertainTypeFromSerialization()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();

            printer.PrintToString(person)
                .Should()
                .NotContain(nameof(person.Age))
                .And
                .NotContain("20");
        }

        [Test]
        public void Excluding_ShouldNotExcludePropertiesWithOtherTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();

            printer.PrintToString(person)
                .Should()
                .Contain(nameof(person.Height))
                .And
                .Contain(nameof(person.Id))
                .And
                .Contain(nameof(person.Name));
        }

        [Test]
        public void PrintingAndUsing_ShouldChangeWayOfSerializingType_WhenInputInUsingIsFunction()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(number => "X");

            printer.PrintToString(person)
                .Should()
                .Contain("X")
                .And
                .Contain(nameof(person.Height));
        }

        [Test]
        public void PrintingAndUsing_ShouldNotChangeWayOfSerializingOtherTypes_WhenInputInUsingIsFunction()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(number => "X");

            printer.PrintToString(person)
                .Should()
                .Contain(person.Name)
                .And
                .Contain(person.Age.ToString());
        }

        [TestCaseSource(nameof(UsingShouldChangeNumberCultureWhenInputIsCultureInfoTestCases))]
        public void PrintingAndUsing_ShouldChangeNumberCulture_WhenInputInUsingIsCultureInfo(CultureInfo cultureInfo)
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(cultureInfo);

            printer.PrintToString(person)
                .Should()
                .Contain(person.Height.ToString(cultureInfo));
        }

        private static IEnumerable UsingShouldChangeNumberCultureWhenInputIsCultureInfoTestCases
        {
            get
            {
                yield return new TestCaseData(CultureInfo.InvariantCulture).SetName("invariant culture");
                yield return new TestCaseData(new CultureInfo("en-GB")).SetName("en-GB culture");
            }
        }

        [Test]
        public void PrintingAndUsing_ShouldChangeWayOfPropertySerialization_WhenInputInPrintingIsPropertyGet()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .Using(name => $"{name}!");

            printer.PrintToString(person)
                .Should()
                .Contain($"{person.Name}!");
        }

        [Test]
        public void PrintingAndUsing_ShouldChangeWayOfSerializationOnlyForSelectedProperty_WhenInputInPrintingIsPropertyGet()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .Using(name => $"{name}!");

            printer.PrintToString(person)
                .Should()
                .Contain($"{person.Name}!")
                .And
                .NotContain($"{person.Surname}!");
        }
    }
}