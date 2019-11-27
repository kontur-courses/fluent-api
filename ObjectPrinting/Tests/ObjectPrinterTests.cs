using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Age = 20, Height = 185.5, Name = "Alex" };
        }

        [Test]
        public void Printer_ShouldPrintEverything_WithoutConfiguration()
        {
            var printer = ObjectPrinter.For<Person>();
            var result = printer.PrintToString(person);
            result.Should().Contain("185,5").And.Contain("Alex").And.Contain("20");
        }

        [Test]
        public void Excluding_ShouldExcludeType_OnInt()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();
            var result = printer.PrintToString(person);
            result.Should().Contain("185,5").And.Contain("Alex").And.NotContain("20");
        }

        [Test]
        public void Excluding_ShouldExcludeProperty_OnName()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Name);
            var result = printer.PrintToString(person);
            result.Should().Contain("185,5").And.Contain("20").And.NotContain("Alex");
        }
    }
}
