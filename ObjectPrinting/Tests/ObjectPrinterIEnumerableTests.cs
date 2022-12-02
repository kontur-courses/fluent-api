using System;
using System.Globalization;
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
            var r = new LibraryRecords()
            {
                ActiveUsers = {"Alex", "Mary", "Stan"},
                ReadersVisits = {{"Alex", new DateTime(1, 1, 1)}, {"Winona", new DateTime(3, 3, 3)}}
            };
            var printer = ObjectPrinter.For<LibraryRecords>();
            Approvals.Verify(printer.PrintToString(r));
        }
        
        [Test]
        public void SetCulture_ShouldWork_ForCollectionsElements()
        {
            var r = new LibraryRecords()
            {
                ActiveUsers = {"Alex", "Mary", "Stan"},
                ReadersVisits = {{"Alex", new DateTime(1, 1, 1)}, {"Winona", new DateTime(3, 3, 3)}}
            };
            var printer = ObjectPrinter.For<LibraryRecords>()
                .SetCulture(new CultureInfo("en"));
            Approvals.Verify(printer.PrintToString(r));
        }
        
        [Test]
        public void SetSerializeMethodForType_ShouldWork_ForCollectionsElements()
        {
            var r = new LibraryRecords()
            {
                ActiveUsers = {"Alex", "Mary", "Stan"},
                ReadersVisits = {{"Alex", new DateTime(1, 1, 1)}, {"Winona", new DateTime(3, 3, 3)}}
            };
            var printer = ObjectPrinter.For<LibraryRecords>()
                .SetSerializeMethodForType<string>(s => s + "123");
            Approvals.Verify(printer.PrintToString(r));
        }
        
        [Test]
        public void ExcludeType_ShouldExclude_CollectionsElements()
        {
            var r = new LibraryRecords()
            {
                ActiveUsers = {"Alex", "Mary", "Stan"},
                ReadersVisits = {{"Alex", new DateTime(1, 1, 1)}, {"Winona", new DateTime(3, 3, 3)}}
            };
            var printer = ObjectPrinter.For<LibraryRecords>()
                .ExcludeType<DateTime>();
            Approvals.Verify(printer.PrintToString(r));
        }
        
        [Test]
        public void Print_ShouldSerializeAsEmpty_EmptyIEnumerable_Or_WithExcludedType()
        {
            var r = new LibraryRecords()
            {
                ReadersVisits = {{"Alex", new DateTime(1, 1, 1)}, {"Winona", new DateTime(3, 3, 3)}}
            };
            var printer = ObjectPrinter.For<LibraryRecords>()
                .ExcludeType<string>();
            Approvals.Verify(printer.PrintToString(r));
        }
        
    }
}