using System;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterCultureInfoTests
    {
        [Test]
        public void SetCulture_Should_ChangeFormat()
        {
            var operation = new Operation
            {
                Operator1 = 0.1f, Operator2 = 0.2f, Operator3 = 0.3f, Operator4 = 0.4, Operator5 = 0.5, Operator6 = 0.6,
                OperationDate = new DateTime(1994, 5, 3)
            };
            var cultureInfo = new CultureInfo("ru");
            var printer = ObjectPrinter.For<Operation>()
                .SetCulture(cultureInfo);
            Approvals.Verify(printer.PrintToString(operation));
        }

        [Test]
        public void SetCulture_Should_OverridePreviousSetCulture()
        {
            var operation = new Operation
            {
                Operator1 = 0.1f, Operator2 = 0.2f, Operator3 = 0.3f, Operator4 = 0.4, Operator5 = 0.5, Operator6 = 0.6,
                OperationDate = new DateTime(1994, 5, 3)
            };
            var cultureInfo1 = new CultureInfo("ru");
            var cultureInfo2 = new CultureInfo("en");
            var printer = ObjectPrinter.For<Operation>()
                .SetCulture(cultureInfo1)
                .SetCulture(cultureInfo2);
            Approvals.Verify(printer.PrintToString(operation));
        }

        [Test]
        public void SetCulture_ShouldNot_OverridePreviousMethod()
        {
            var operation = new Operation
            {
                Operator1 = 0.1f, Operator2 = 0.2f, Operator3 = 0.3f, Operator4 = 0.4, Operator5 = 0.5, Operator6 = 0.6,
                OperationDate = new DateTime(1994, 5, 3)
            };
            var cultureInfo = new CultureInfo("ru");
            var printer = ObjectPrinter.For<Operation>()
                .ConfigForProperty(op => op.Operator1)
                .UseSerializeMethod(f => "123")
                .SetCulture(cultureInfo);
            Approvals.Verify(printer.PrintToString(operation));
        }

        [Test]
        public void UseSerializeMethod_Should_OverridePreviousSetCulture()
        {
            var operation = new Operation
            {
                Operator1 = 0.1f, Operator2 = 0.2f, Operator3 = 0.3f, Operator4 = 0.4, Operator5 = 0.5, Operator6 = 0.6,
                OperationDate = new DateTime(1994, 5, 3)
            };
            var cultureInfo = new CultureInfo("ru");
            var printer = ObjectPrinter.For<Operation>()
                .SetCulture(cultureInfo)
                .ConfigForProperty(op => op.Operator1)
                .UseSerializeMethod(f => "123");
            Approvals.Verify(printer.PrintToString(operation));
        }
    }
}