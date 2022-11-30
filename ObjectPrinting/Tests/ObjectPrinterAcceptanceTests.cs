using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseApprovalSubdirectory("ObjectPrinterAcceptanceTests")]
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>();
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ExcludeProperty_Should_Exclude()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(p => p.Id);
            Approvals.Verify(printer.PrintToString(person));
        }
        
        [Test]
        public void OverrideProperty_Should_OverrideCorrectly()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Age)
                .UseSerializeMethod(i => "Old");
            Approvals.Verify(printer.PrintToString(person));
        }
        
        [Test]
        public void OverrideProperty_Should_DiscardPrevOverrideProperty()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Age)
                .UseSerializeMethod(i => "Old")
                .ConfigForProperty(p => p.Age)
                .UseSerializeMethod(i => i + " Old");
            Approvals.Verify(printer.PrintToString(person));
        }
        
        [Test]
        public void OverrideProperty_ShouldNot_Work_OnExcludedProperty()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(p => p.Age)
                .ConfigForProperty(p => p.Age)
                .UseSerializeMethod(i => "Old");
            Approvals.Verify(printer.PrintToString(person));
        }
    }
}