using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrinterTest
{
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
            var testPerson = new TestClass() {Name = "test"};
            var result = testPerson.PrintToString();

            result.Should()
                .Be(
                    $"TestClass{Environment.NewLine}\tA = null{Environment.NewLine}\tName = test{Environment.NewLine}");
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
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>().Excluding(t => t.Name)
                .Printing<double>()
                .Using(CultureInfo.GetCultureInfo("ru-RU"));

            var result = printer.PrintToString(person);

            result.Should().Be($"Person{Environment.NewLine}\tHeight = 2,123{Environment.NewLine}\tAge = 0{Environment.NewLine}");
        }

        [Test]
        public void Printer_ShouldTrimmedLength()
        {
            var testClass = new TestClass{Name = "very big text"};
            var printer = ObjectPrinter.For<TestClass>().Printing(tc => tc.Name).TrimmedToLength(4);

            var result = printer.PrintToString(testClass);
            
            result.Should().Be($"TestClass{Environment.NewLine}\tA = null{Environment.NewLine}\tName = very{Environment.NewLine}");
        }

        [Test]
        public void Printer_ShouldWork_WhenRecursion()
        {
            var testClass = new TestClass(){ Name = "test"};
            testClass.A = testClass;
            var printer = ObjectPrinter.For<TestClass>();

            var result = printer.PrintToString(testClass);
            
            result.Should().Be($"TestClass{Environment.NewLine}\tA = itself{Environment.NewLine}\tName = test{Environment.NewLine}");
        }

        [Test]
        public void Printer_ShouldPrintIEnumerable()
        {
            var enumerable = new List<int>{1,2,3};
            
            var result = enumerable.PrintToString();
            
            result.Should().Be($"[1, 2, 3]{Environment.NewLine}");
        }

        [Test]
        public void Printer_ShouldPrintDictionary()
        {
            var dictionary = new Dictionary<int, string>{{1, "one"}, {2, "two"}};

            var result = dictionary.PrintToString();
            
            Console.WriteLine(result);
        }

        [Test]
        public void Printer_ShouldPrintFieldsAndProperty()
        {
            var testClass = new ClassWithFieldsAndProperty{Field1 = "one", Field2 = 2};
            testClass.Field3 = testClass;
            var printer = ObjectPrinter.For<ClassWithFieldsAndProperty>();
            var expectedResult = $"ClassWithFieldsAndProperty{Environment.NewLine}\tProperty = null{Environment.NewLine}\tField1 = one{Environment.NewLine}\tField2 = 2{Environment.NewLine}\tField3 = itself{Environment.NewLine}";

            var result = printer.PrintToString(testClass);
            
            result.Should().Be(expectedResult);
        }
    }
}