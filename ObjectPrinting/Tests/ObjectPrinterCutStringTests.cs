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
        public void CutString_Should_CutCorrectly_WhenStringIsLarge()
        {
            var person = new Person {Name = "Alexander", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Name)
                .CutString(4);
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void CutString_ShouldNot_Cut_WhenStringIsSmall()
        {
            var person = new Person {Name = "Al", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Name)
                .CutString(4);
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void CutString_ShouldNot_Cut_WhenStringIsEqualToMaxLength()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Name)
                .CutString(4);
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void CutString_ShouldNot_Override_OtherSerializeMethod()
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
        public void OverrideStringSerializeMethod_ShouldNot_Override_CutString()
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
        public void CutString_Should_Override_PrevCutString()
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