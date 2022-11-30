using System;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseApprovalSubdirectory("ObjectPrinterCulturalInfoTests")]
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterCultureInfoTests
    {
        [Test]
        public void SetCulture_Should_ChangeFormat()
        {
            var operation = new Operation()
            {
                Operator1 = 0.1f, Operator2 = 0.2f, Operator3 = 0.3f, Operator4 = 0.4, Operator5 = 0.5, Operator6 = 0.6,
                OperationDate = new DateTime(1994, 5, 3)
            };
            CultureInfo cultureInfo = new CultureInfo("ru");
            var printer = ObjectPrinter.For<Operation>()
                .SetCulture(cultureInfo);
            Approvals.Verify(printer.PrintToString(operation));
        }
    }
}