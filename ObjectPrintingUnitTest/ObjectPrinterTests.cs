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
    }
}
