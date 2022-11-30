using System;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseApprovalSubdirectory("ObjectPrinterForTypeMethods")]
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterForTypeMethods
    {
        [Test]
        public void ExcludeType_Should_Exclude()
        {
            var operation = new Operation()
            {
                Operator1 = 0.1f, Operator2 = 0.2f, Operator3 = 0.3f, Operator4 = 0.4, Operator5 = 0.5, Operator6 = 0.6,
                OperationDate = new DateTime(1994, 5, 3)
            };
            var printer = ObjectPrinter.For<Operation>()
                .ExcludeType<float>();
            Approvals.Verify(printer.PrintToString(operation));
        }
        
        [Test]
        public void AlternateType_Should_Alternate()
        {
            var operation = new Operation()
            {
                Operator1 = 0.1f, Operator2 = 0.2f, Operator3 = 0.3f, Operator4 = 0.4, Operator5 = 0.5, Operator6 = 0.6,
                OperationDate = new DateTime(1994, 5, 3)
            };
            var printer = ObjectPrinter.For<Operation>()
                .SetMethodForType((double d) => d + " hehe");
            Approvals.Verify(printer.PrintToString(operation));
        }
    }
}