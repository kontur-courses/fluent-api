using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterCutStringTests
    {
        [Test]
        public void CutString_Should_CutCorrectly_WhenStringIsLarger_ThanMaxLength()
        {
            var person = new Person {Name = "Alexander", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Name)
                .CutString(4);
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void CutString_ShouldNot_Cut_WhenStringIsSmaller_ThanMaxLength()
        {
            var person = new Person {Name = "Al", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Name)
                .CutString(4);
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void CutString_ShouldNot_Cut_WhenStringIsEqual_ToMaxLength()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Name)
                .CutString(4);
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void CutString_Should_Work_And_ShouldNot_OverrideOtherSerializeMethod()
        {
            var person = new Person {Name = "Alexander", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Name)
                .UseSerializeMethod(s => "Call me " + s)
                .ConfigForProperty(p => p.Name)
                .CutString(4);
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void UseSerializeMethod_Should_Work_And_ShouldNot_OverrideCutString()
        {
            var person = new Person {Name = "Alexander", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Name)
                .CutString(4)
                .ConfigForProperty(p => p.Name)
                .UseSerializeMethod(s => "Call me " + s);
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void CutString_Should_Override_PreviousCutString()
        {
            var person = new Person {Name = "Alexander", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Name)
                .CutString(2)
                .ConfigForProperty(p => p.Name)
                .CutString(4);
            Approvals.Verify(printer.PrintToString(person));
        }
    }
}