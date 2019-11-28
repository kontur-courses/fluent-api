using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class ObjectPrinterTests
    {
        private static Person person;
        private static PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            person = new Person("Alex", 15, 123.1, new[] {2, 2, 2, 5,});
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ObjectPrinter_DoubleField_ShouldHaveChangedCulture()
        {
            var line = printer.For<double>().WithCulture(CultureInfo.CurrentCulture).PrintToString(person);

            Assert.Fail();
        }

        [Test]
        public void ObjectPrinter_ExcludingField_NotPrinting()
        {
            var line = printer.Excluding<string>().PrintToString(person);

            line.Should().NotContain("Alex");
        }

        [Test]
        public void ObjectPrinter_ExcludingTypeFields_NotPrinting()
        {
            var line = printer.Excluding(p => p.Name).PrintToString(person);

            line.Should().NotContain("Alex");
        }

        [Test]
        public void ObjectPrinter_TrimField_Changed()
        {
            var line = printer.For<string>().WithSerialization(p => p.ToString() + " string changed").PrintToString(person);

            line.Should().Contain("string changed");
        }

        [Test]
        public void ObjectPrinter_DoubleField_HasLessOrEqualLengthAsTrim()
        {
            var line = printer.For(p => p.Name).Trim(3).PrintToString(person);

            line.Should().NotContain("Alex");
            line.Should().Contain("Ale");
        }
    }
}