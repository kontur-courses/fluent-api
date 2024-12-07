using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Solved.Tests;

namespace ObjectPrintingTests
{
    internal class PrinterExtensionsTests
    {
        [Test]
        public void WhenCreatePrinterWithExtensionMethod_ShouldNotBeNull()
        {
            var person = new Person();
            var printer = person.CreatePrinter();

            printer.Should().NotBe(null);
        }
    }
}