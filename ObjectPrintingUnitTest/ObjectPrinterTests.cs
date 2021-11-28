using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Solved.Tests;
using FluentAssertions;
namespace ObjectPrintingUnitTest
{
    public class ObjectPrinterTests
    {
        private Person person;
        [SetUp]
        public void SetUp()
        {
            person = new Person { Id = Guid.NewGuid(), Name = "Maxim", Age = 21, Height = 180.0};
        }

        [Test]
        public void PrintToString_ShouldExcludedTypeGuid()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();

            printer.PrintToString(person).Should().Be("Person\r\n\tName = Maxim\r\n\tHeight = 180\r\n\tAge = 21\r\n");
        }

        [Test]
        public void PrintToString_ShouldExcludedMember()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Id);

            printer.PrintToString(person).Should().Be("Person\r\n\tName = Maxim\r\n\tHeight = 180\r\n\tAge = 21\r\n");
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

            printer.PrintToString(person).Should().Be("Person\r\n\tId = Guid\r\n\tName = Maxim\r\n\tHeight = 180\r\n\tAge = 21 years old\r\n\t");

        [Test]
        public void PrintToString_SetCulture_ShouldBe_DoubleWithDot()
        {
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString(person).Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = Maxim\r\n\tHeight = 180.2\r\n\tAge = 21\r\n");
        }

    }
}
