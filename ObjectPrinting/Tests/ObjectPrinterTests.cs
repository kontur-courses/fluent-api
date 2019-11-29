using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests_Should
    {
        public PrintingConfig<PrinterTestContainer> Printer;
        public PrinterTestContainer Container = new PrinterTestContainer();

        [SetUp]
        public void SetUp()
        {
            Printer = ObjectPrinter.For<PrinterTestContainer>();
        }

        [Test]
        public void ExcludePropertiesOfTheSameType()
        {
            Printer.Excluding<int>().PrintObject(Container).Should().NotContain("I");
        }

        [Test]
        public void ExcludeGivenProperty()
        {
            Printer.Excluding(c => c.I1).PrintObject(Container).Should().NotContain("I1");
        }

        [Test]
        public void ExcludeOnlyGivenProperty()
        {
            Printer.Excluding(c => c.I1).PrintObject(Container).Should().Contain("I2").And.Contain("I3");
        }

        [Test]
        public void UsesGivenSerializerForType()
        {
            Printer.Serializing<double>()
                .Using(d => d + "d")
                .PrintObject(Container)
                .Should()
                .Contain("d");
        }

        [Test]
        public void UsesGivenSerializerForGivenProperty()
        {
            Printer.Serializing(c => c.D1)
                .Using(d => d + "d")
                .PrintObject(Container)
                .Should()
                .Contain("0.5d");
        }

        [Test]
        public void UsesGivenSerializerOnlyForGivenProperty()
        {
            Printer.Serializing(c => c.D1)
                .Using(d => d + "d")
                .PrintObject(Container)
                .Should()
                .NotContain("1.5d")
                .And
                .NotContain("2.5d");
        }

        [Test]
        public void EnableToSpecifyDoubleCulture()
        {
            var culture = CultureInfo.CreateSpecificCulture("Rus");
            culture.NumberFormat = new NumberFormatInfo {NumberDecimalSeparator = ","};
            Printer.Serializing<double>()
                .Using(culture)
                .PrintObject(Container)
                .Should()
                .Contain("0,5");
        }

        [Test]
        public void ChangeFormatForType()
        {
            Printer.Serializing<double>()
                .UsingFormat((indent, propertyName, serializedProperty) =>
                    indent + propertyName + "&" + serializedProperty)
                .PrintObject(Container)
                .Should()
                .Contain("D1&0.5");
        }

        [Test]
        public void ChangeFormatForProperty()
        {
            Printer.Serializing(t => t.D2)
                .UsingFormat((context) =>
                    context.indent + context.propertyName + "&" + context.serializedProperty)
                .PrintObject(Container)
                .Should()
                .Contain("D2&1.5");
        }

        [Test]
        public void ChangeFormatOnlyForGivenProperty()
        {
            Printer.Serializing(t => t.D2)
                .UsingFormat((context) =>
                    context.indent + context.propertyName + "&" + context.serializedProperty)
                .PrintObject(Container)
                .Should()
                .NotContain("D1&0.5")
                .And
                .NotContain("D3&2.5");
        }
    }
}