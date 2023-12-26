using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    internal class PrinterExtensionsTests
    {
        [Test]
        public void CreatePrinterWithExtensionMethod_ShouldNotBeNull()
        {
            var person = new Person();
            var printer = person.CreatePrinter();

            printer.Should().NotBe(null);
        }
    }
}
