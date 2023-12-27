using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 180.5 };

            var printer = ObjectPrinter.For<Person>();

            var actualString = printer.Exclude<SubPerson>()
                .Printing(p => p.Age)
                .Using(age => (age + 1000).ToString())
                .And.Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .And.Printing(p => p.Name)
                .Trim(1)
                .And.Exclude(p => p.PublicField)
                .OnMaxRecursion((_) => throw new ArgumentException())
                .PrintToString(person);

            actualString.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = A\r\n\tHeight = 180.5\r\n\tAge = 1019\r\n");
        }
    }
}