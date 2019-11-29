using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    class PrintingConfigTests
    {
        [Test]
        public void Printing_ShouldThrowArgumentException_IfNotMemberExpression()
        {
            var printer = ObjectPrinter.For<Person>();
            Action action = () => printer.Printing(p => 1 + 2);

            action.ShouldThrow<ArgumentException>();
        }
    }
}
