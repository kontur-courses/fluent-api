using System;
using NUnit.Framework;
using ObjectPrinting.Solved.TestData;

namespace ObjectPrinting.Solved.Tests
{
    public class ObjectPrinterTests
    {
        private Person person;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            person = Person.GetInstance();
            person.Parent = Person.GetInstance();
        }

        [Test]
        public void ObjPrinter_ShouldExcludeSpecificType()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.Excluding<Guid>();

            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void ObjPrinter_ShouldExcludeSpecificProperty()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.Excluding(p => p.Age);

            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void ObjPrinter_ShouldExcludeSpecificField()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.Excluding(p => p.SomeField);

            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void ObjPrinter_ShouldUseAlternativeScenario_WithField()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.PrintingMember(p => p.SomeField).Using(d => $"Some field is {d}");

            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void ObjPrinter_ShouldUseAlternativeScenario_WithProperty()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.PrintingMember(p => p.Age).Using(a => $"The age is {a}");

            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void ObjPrinter_ShouldUseAlternativeScenario_WithType()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.PrintingType<Guid>().Using(g => $"The guid is {g}");

            Console.WriteLine(printer.PrintToString(person));
        }
    }
}