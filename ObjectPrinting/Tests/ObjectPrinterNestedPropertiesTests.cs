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
            var p = new Person() {Name = "Alex", Height = 35.5, Weight = 83.3};
            var parent = new Person() {Name = "Masha", Height = 53, Weight = 53};
            p.Parent = parent;
            var printer = ObjectPrinter.For<Person>();
            Approvals.Verify(printer.PrintToString(p));
        }

        [Test]
        public void Print_ShouldNot_Serialize_ExcludedNestedProperties()
        {
            var p = new Person() {Name = "Alex", Height = 35.5, Weight = 83.3};
            var parent = new Person() {Name = "Masha", Height = 53, Weight = 53};
            p.Parent = parent;
            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(p => p.Parent.Age);
            Approvals.Verify(printer.PrintToString(p));
        }

        [Test]
        public void ExcludeType_Should_Exclude_OnEveryLevel()
        {
            var p = new Person() {Name = "Alex", Height = 35.5, Weight = 83.3};
            var parent = new Person() {Name = "Masha", Height = 53, Weight = 53};
            p.Parent = parent;
            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<Guid>();
            Approvals.Verify(printer.PrintToString(p));
        }

        [Test]
        public void SetCulture_Should_Set_OnEveryLevel()
        {
            var p = new Person() {Name = "Alex", Height = 35.5, Weight = 83.3};
            var parent = new Person() {Name = "Masha", Height = 53, Weight = 53};
            p.Parent = parent;
            var printer = ObjectPrinter.For<Person>()
                .SetCulture(new CultureInfo("ru"));
            Approvals.Verify(printer.PrintToString(p));
        }

        [Test]
        public void UseSerializeMethod_ShouldNot_Work_OnOtherLevels()
        {
            var p = new Person() {Name = "Alex", Height = 35.5, Weight = 83.3};
            var parent = new Person() {Name = "Masha", Height = 53, Weight = 53};
            p.Parent = parent;
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p=>p.Age)
                .UseSerializeMethod(i => "Old");
            Approvals.Verify(printer.PrintToString(p));
        }
    }
}