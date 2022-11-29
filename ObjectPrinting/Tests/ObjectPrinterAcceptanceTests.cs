using ApprovalTests;
using ApprovalTests.Core;
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
                .ConfigForProperty(p => p.Id)
                .ExcludeFromConfig();
            Approvals.Verify(printer.PrintToString(person));
        }
        
        [Test]
        public void OverrideProperty_Should_OverrideCorrectly()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Age)
                .OverrideSerializeMethod(i => "Old")
                .SetConfig();
            Approvals.Verify(printer.PrintToString(person));
        }
        
        [Test]
        public void OverrideProperty_Should_DiscardPrevOverrideProperty()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Age)
                .OverrideSerializeMethod(i => "Old")
                .OverrideSerializeMethod(i=>i+" Old")
                .SetConfig();
            Approvals.Verify(printer.PrintToString(person));
        }
        
        [Test]
        public void OverrideProperty_ShouldNot_Work_OnExcludedProperty()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p=>p.Age)
                .ExcludeFromConfig()
                .ConfigForProperty(p => p.Age)
                .OverrideSerializeMethod(i => "Old")
                .SetConfig();
            Approvals.Verify(printer.PrintToString(person));
        }
    }
}