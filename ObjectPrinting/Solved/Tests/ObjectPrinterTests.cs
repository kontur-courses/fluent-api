using System;
using NUnit.Framework;
using ObjectPrinting.Solved.TestData;

namespace ObjectPrinting.Solved.Tests
{
    public class ObjectPrinterTests
    {
        private readonly Person person = Person.GetInstance();

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
    }
}