using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterForTypeMethods
    {
        [Test]
        public void ExcludeType_Should_Exclude()
        {
            var p = new Person() {Name = "Alex", Height = 35.5, Weight = 83.3};
            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<double>();
            Approvals.Verify(printer.PrintToString(p));
        }

        [Test]
        public void AlternateType_Should_Alternate()
        {
            var p = new Person() {Name = "Alex", Height = 35.5, Weight = 83.3};
            var printer = ObjectPrinter.For<Person>()
                .SetSerializeMethodForType<double>(d => d + " hehe");
            Approvals.Verify(printer.PrintToString(p));
        }
    }
}