using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Solved;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [TestCase]
        public void ObjectPrinter_ExcludingType()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();
            var result = printer.PrintToString(person);
            result.Should().NotContain("Guid");
        }
        
        [TestCase]
        public void ObjectPrinter_ExcludingMember()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Name);
            var result = printer.PrintToString(person);
            result.Should().NotContain("Name");
        }
    }
}