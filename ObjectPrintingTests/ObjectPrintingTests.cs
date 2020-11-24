using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

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
                .Printing(p => p.Name);
        }

        [Test]
        public void AlternateSerializationForType_Exists()
        {
            var _ = printer
                .Printing<int>()
                .Using(i => i);
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
            var printer = ObjectPrinter.For<Person>();

            var _ = this.printer.Excluding(p => p.Name);
        }
    }
}