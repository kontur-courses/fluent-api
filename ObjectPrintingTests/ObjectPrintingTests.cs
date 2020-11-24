using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Infrastructure;
using ObjectPrintingTests.Mocks;

namespace ObjectPrintingTests
{
    public class Tests
    {
        private PrintingConfig<object> printer;
        
        [SetUp]
        public void Setup()
        {
            printer = ObjectPrinter.For<object>();
        }

        [Test]
        public void TypeExcluding_Exists()
        {
            var _ = printer.Excluding<string>();
        }

        [Test]
        public void SelectorForPropertyOrField_Exists()
        {
            var _ = printer
                .Printing<object>();
        }

        [Test]
        public void AlternateSerializationForType_Exists()
        {
            var _ = printer
                .Printing<int>()
                .Using(i => "i");
        }

        [Test]
        public void CultureInfoForIFormattable_Exists()
        {
            var _ = printer
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture);
        }

        [Test]
        public void StringShortening_Exists()
        {
            var _ = printer
                .Printing<string>()
                .TrimmedToLength(10);
        }

        [Test]
        public void PropertyOrFieldExcluding_Exists()
        {
            var printer = ObjectPrinter.For<SingleProperty>();

            var _ = printer.Excluding(p => p.Property);
        }
        
        [Test]
        public void PrintToString_ExcludingByProperty()
        {
            var expected = GetSystemIndependent("SingleProperty\n");
            var objectToPrint = new SingleProperty() { Property = new{} };
            var printer = ObjectPrinter
                .For<SingleProperty>()
                .Excluding(p => p.Property);

            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_ExcludingByType()
        {
            var expected = GetSystemIndependent("SingleProperty\n");
            var objectToPrint = new SingleProperty() { Property = new{} };
            var printer = ObjectPrinter
                .For<SingleProperty>()
                .Excluding<object>();

            var actual = printer.PrintToString(objectToPrint);

            actual.Should().Be(expected);
        }

        private static string GetSystemIndependent(string text)
        {
            return text.Replace("\n", Environment.NewLine);
        }
    }
}