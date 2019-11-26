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
            Printer.Excluding<int>().PrintWithConfig(Container).Should().NotContain("I");
        }

        [Test]
        public void ExcludeGivenProperty()
        {
            Printer.Excluding(c => c.I1).PrintWithConfig(Container).Should().NotContain("I1");
        }

        [Test]
        public void ExcludeOnlyGivenProperty()
        {
            Printer.Excluding(c => c.I1).PrintWithConfig(Container).Should().Contain("I2").And.Contain("I3");
        }

        [Test]
        public void UsesGivenSerializerForType()
        {
            Printer.Serializing<double>().Using(d => d + "d").PrintWithConfig(Container).Should().Contain("d");
        }

        [Test]
        public void UsesGivenSerializerForGivenProperty()
        {
            Printer.Serializing(c => c.D1).Using(d => d + "d").PrintWithConfig(Container).Should().Contain("0.0d");
        }

        [Test]
        public void UsesGivenSerializerOnlyForGivenProperty()
        {
            Printer.Serializing(c => c.D1).Using(d => d + "d").PrintWithConfig(Container)
                .Should().NotContain("1.0d").And.NotContain("2.0d");
        }
        
        [Test]
        public void EnableToSpecifyDoubleCulture()
        {
            Printer.Serializing<double>().Using(CultureInfo.InvariantCulture).PrintWithConfig(Container).Should()
                .Contain("0.0d");
        }
    }
}