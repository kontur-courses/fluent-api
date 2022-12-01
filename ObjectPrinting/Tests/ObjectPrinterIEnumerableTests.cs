using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterIEnumerableTests
    {
        [Test]
        public void Print_Should_CorrectlyDisplayIEnumerables()
        {
            var e = new Enumerics();
            var printer = ObjectPrinter.For<Enumerics>();
            Approvals.Verify(printer.PrintToString(e));
        }
    }
}