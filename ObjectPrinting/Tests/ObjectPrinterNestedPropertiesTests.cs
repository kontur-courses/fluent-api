using System;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterNestedPropertiesTests
    {
        [Test]
        public void Print_Should_SerializeNestedProperties()
        {
            var no = new NestedObjectOne
            {
                Date = new DateTime(1, 1, 1),
                Name = "Al",
                Ref = new NestedObjectTwo {Birthday = new DateTime(2, 2, 2), Surname = "Fer"}
            };
            var printer = ObjectPrinter.For<NestedObjectOne>();
            Approvals.Verify(printer.PrintToString(no));
        }

        [Test]
        public void Print_ShouldNot_Serialize_ExcludedNestedProperties()
        {
            var no = new NestedObjectOne
            {
                Date = new DateTime(1, 1, 1),
                Name = "Al",
                Ref = new NestedObjectTwo {Birthday = new DateTime(2, 2, 2), Surname = "Fer"}
            };
            var printer = ObjectPrinter.For<NestedObjectOne>()
                .ExcludeProperty(s => s.Ref.Surname);
            Approvals.Verify(printer.PrintToString(no));
        }

        [Test]
        public void ExcludeType_Should_Exclude_OnEveryLevel()
        {
            var no = new NestedObjectOne
            {
                Date = new DateTime(1, 1, 1),
                Name = "Al",
                Ref = new NestedObjectTwo {Birthday = new DateTime(2, 2, 2), Surname = "Fer"}
            };
            var printer = ObjectPrinter.For<NestedObjectOne>()
                .ExcludeType<string>();
            Approvals.Verify(printer.PrintToString(no));
        }

        [Test]
        public void SetCulture_Should_Set_OnEveryLevel()
        {
            var no = new NestedObjectOne
            {
                Date = new DateTime(1, 1, 1),
                Name = "Al",
                Ref = new NestedObjectTwo {Birthday = new DateTime(2, 2, 2), Surname = "Fer"}
            };
            var printer = ObjectPrinter.For<NestedObjectOne>()
                .SetCulture(new CultureInfo("ru"));
            Approvals.Verify(printer.PrintToString(no));
        }

        [Test]
        public void UseSerializeMethod_ShouldNot_Work_OnOtherLevels()
        {
            var cyc = new CyclicReference {ID = 0};
            cyc.Ref = new CyclicReference {ID = 1};
            var printer = ObjectPrinter.For<CyclicReference>()
                .ConfigForProperty(c => c.ID)
                .UseSerializeMethod(i => "123");
            Approvals.Verify(printer.PrintToString(cyc));
        }
    }
}