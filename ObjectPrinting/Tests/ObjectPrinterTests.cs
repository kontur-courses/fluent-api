using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Age = 20, Height = 185.5, Name = "Alex" };
        }

        [Test]
        public void Printer_ShouldPrintEverything_WithoutConfiguration()
        {
            var printer = ObjectPrinter.For<Person>();
            var result = printer.PrintToString(person);
            result.Should().Contain("185,5").And.Contain("Alex").And.Contain("20");
        }

        [Test]
        public void Excluding_ShouldExcludeType_OnInt()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();
            var result = printer.PrintToString(person);
            result.Should().Contain("185,5").And.Contain("Alex").And.NotContain("20");
        }

        [Test]
        public void Excluding_ShouldExcludeProperty_OnName()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Name);
            var result = printer.PrintToString(person);
            result.Should().Contain("185,5").And.Contain("20").And.NotContain("Alex");
        }

        [Test]
        public void Printing_ShouldPrintTypeAsSpecified_OnInt()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(i => "abc");
            var result = printer.PrintToString(person);
            result.Should().Contain("abc").And.NotContain("20");
        }

        [Test]
        public void Printing_ShouldPrintTypeWithSpecifiedCulture_OnDouble()
        {
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture);
            var result = printer.PrintToString(person);
            result.Should().NotContain("185,5").And.Contain("185.5");
        }

        [Test]
        public void Printing_ShouldReturnSecondGivenFormat_OnTwoFormats()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(d => "double")
                .Printing<double>().Using(CultureInfo.InvariantCulture);
            var result = printer.PrintToString(person);
            result.Should().NotContain("185,5").And.NotContain("double").And.Contain("185.5");
        }

        [Test]
        public void Printing_ShouldPrintPropertyAsSpecified_OnName()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).Using(n => $"Имя: {n}");
            var result = printer.PrintToString(person);
            result.Should().Contain("Имя: Alex").And.NotContain("Name = Alex");
        }

        [Test]
        public void TrimmedToLength_ShouldPrintTrimmedProperty_OnLongProperty()
        {
            person.Name = "abcdefghijklmnop";
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(4);
            var result = printer.PrintToString(person);
            result.Should().Contain("abcd").And.NotContain("abcdefghijklmnop");
        }

        [Test]
        public void ExtensionPrintToString_ShouldSerializeAsUsualPrintToString_WithoutConfiguration()
        {
            var printer = ObjectPrinter.For<Person>();
            var expectedResult = printer.PrintToString(person);

            var result = person.PrintToString();

            result.Should().Be(expectedResult);
        }

        [Test]
        public void ExtensionPrintToString_ShouldSerializeAsUsualPrintToString_WithConfiguration()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).Using(n => $"Имя: {n}");
            var expectedResult = printer.PrintToString(person);

            var result = person.PrintToString(c => c.Printing(p => p.Name).Using(n => $"Имя: {n}"));

            result.Should().Be(expectedResult);
        }

        [Test]
        public void PrintToString_ShouldCorrectlyProcessCyclicReferences()
        {
            var person1 = new Person {Name = "Alice"};
            var person2 = new Person { Name = "Bob", Partner = person1};
            person1.Partner = person2;

            var result = person1.PrintToString();

            result.Should().Contain("Partner contains cyclic reference");
        }
    }
}
