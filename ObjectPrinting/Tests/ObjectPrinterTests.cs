using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class TestClass
    {
        public TestClass A { get; set; }
        public string Name { get; set; }
    }

    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void Printer_ShouldAddSpecialSerializeFunctionForType()
        {
            var testClass = new TestClass {Name = "test"};
            var printer = ObjectPrinter.For<TestClass>().Printing<TestClass>().Using(f => "done");

            var result = printer.PrintToString(testClass);

            result.Should()
                .Be(
                    $"TestClass{Environment.NewLine}\tA = done{Environment.NewLine}\tName = {testClass.Name}{Environment.NewLine}");
        }

        [Test]
        public void Printer_ShouldExcludingProperty()
        {
            var testClass = new TestClass {Name = "test", A = null};
            var printer = ObjectPrinter.For<TestClass>().Excluding(t => t.A);

            var result = printer.PrintToString(testClass);

            result.Should().Be($"TestClass{Environment.NewLine}\tName = {testClass.Name}{Environment.NewLine}");
        }

        [Test]
        public void Printer_ShouldExcludingType()
        {
            var testPerson = new Person {Name = "aga"};
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>().Excluding<double>().Excluding<int>();

            var result = printer.PrintToString(testPerson);

            result.Should().Be($"Person{Environment.NewLine}\tName = {testPerson.Name}{Environment.NewLine}");
        }

        [Test]
        public void Printer_ShouldPrintAllPropertyByDefault()
        {
            var testPerson = new Person {Name = "test"};
            var result = testPerson.PrintToString();

            result.Should()
                .Be(
                    $"Person{Environment.NewLine}\tId = Guid{Environment.NewLine}\tName = test{Environment.NewLine}\tHeight = 0{Environment.NewLine}\tAge = 0{Environment.NewLine}");
        }

        [Test]
        public void PrinterFor_ShouldReturnPrintingConfig()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.Should().BeOfType(typeof(PrintingConfig<Person>));
        }

        [Test]
        public void Printer_ShouldChangeCulture()
        {
            var person = new Person { Height = 2.123};
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.GetCultureInfo("ru-RU"));

            var result = printer.PrintToString(person);

            result.Should().Be($"Person{Environment.NewLine}\tId = Guid{Environment.NewLine}\tName = null{Environment.NewLine}\tHeight = 2,123{Environment.NewLine}\tAge = 0{Environment.NewLine}");
        }

        [Test]
        public void Printer_ShouldTrimmedLength()
        {
            var testClass = new TestClass{Name = "very big text"};
            var printer = ObjectPrinter.For<TestClass>().Printing(tc => tc.Name).TrimmedToLength(4);

            var result = printer.PrintToString(testClass);
            
            result.Should().Be($"TestClass{Environment.NewLine}\tA = null{Environment.NewLine}\tName = very{Environment.NewLine}");
        }
    }
}