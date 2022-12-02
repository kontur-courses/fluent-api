using System;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person
            {
                Name = "Alexander", Age = 19, Birthday = new DateTime(2011, 11, 11),
                Height = 155, Weight = 40, RunRatio = 4
            };
            var parent = new Person
            {
                Name = "Dudeness", Age = 40, Birthday = new DateTime(1990, 11, 11),
                Height = 160, Weight = 80, RunRatio = 2
            };
            person.Parent = parent;
            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<double>()
                .SetSerializeMethodForType<string>(s => s + ", Captain " + s)
                .SetCulture(new CultureInfo("ru"))
                .ConfigForProperty(p => p.Parent.Age).UseSerializeMethod(d => "Old")
                .ConfigForProperty(p => p.Parent.Name).CutString(4)
                .ExcludeProperty(p => p.Id);
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
        public void UseSerializeMethod_Should_Work()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ConfigForProperty(p => p.Age)
                .UseSerializeMethod(i => "Old");
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void UseSerializeMethod_Should_DiscardPreviousOne()
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
        public void UseSerializeMethod_ShouldNot_Work_OnExcludedProperty()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(p => p.Age)
                .ConfigForProperty(p => p.Age)
                .UseSerializeMethod(i => "Old");
            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void Print_ShouldSerialize_WhenCyclicReference()
        {
            var p1 = new Person {Name = "Alex"};
            var p2 = new Person {Name = "Boris"};
            p1.Parent = p2;
            p2.Parent = p1;

            var printer = ObjectPrinter.For<Person>();
            Approvals.Verify(printer.PrintToString(p1));
        }
    }
}