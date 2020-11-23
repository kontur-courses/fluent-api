using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace Tests
{
    public class ObjectPrinterTests
    {
        private PrintingConfig<Person> printer;
        private Person testSubject;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            testSubject = new Person {Age = 18, Height = 123, Id = Guid.NewGuid(), Name = "Abc xyz"};
        }

        [Test]
        public void CanPrintFieldsAndProperties()
        {
            PerformTest("Person", $"Id = {testSubject.Id}", $"Name = {testSubject.Name}",
                $"Height = {testSubject.Height}", $"Age = {testSubject.Age}");
        }

        private void PerformTest(params string[] expectedParts)
        {
            var result = printer.PrintToString(testSubject);
            result.Should()
                .ContainAll(expectedParts);
            TestContext.Out.WriteLine(result);
        }
    }
}