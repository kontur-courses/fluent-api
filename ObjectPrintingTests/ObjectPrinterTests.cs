using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{

    class ObjectPrinterTests
    {
        private String newLine = Environment.NewLine;

        [Test]
        public void Excluding_ShouldExcludeType()
        {
            var doubleWrapper = new DoubleWrapper(10);
            var expected = $"DoubleWrapper{newLine}";
            var actual = ObjectPrinter.For<DoubleWrapper>()
                .Excluding<double>()
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Excluding_ShouldExcludeProperty()
        {
            var doubleWrapper = new DoubleWrapper(10);
            var expected = $"DoubleWrapper{newLine}";
            var actual = ObjectPrinter.For<DoubleWrapper>()
                .Excluding(wrapper => wrapper.Value)
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

        [TestCase("ru-RU", "10,1")]
        [TestCase("en-US", "10.1")]
        public void UsingCultureInfo_ShouldSetCulture(string cultureName, string expectedDoubleRepr)
        {
            var doubleWrapper = new DoubleWrapper(10.1);
            var expected = $"DoubleWrapper{newLine}\tValue = {expectedDoubleRepr}{newLine}";
            var actual = ObjectPrinter.For<DoubleWrapper>()
                .Printing<double>().Using(new CultureInfo(cultureName))
                .PrintToString(doubleWrapper);

            Assert.AreEqual(expected, actual);
        }

    }
}
