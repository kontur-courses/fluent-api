using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting
{

    public class ExampleClass<T>
    {
        public T Field { get; set; }
    }
    
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [Test]
        public void Printer_ForShoudleReturnPrintingConfig()
        {
            var testClass = new ExampleClass<int>();
            var printingConfig = ObjectPrinter.For<ExampleClass<int>>();
            
            printingConfig.Should().BeOfType<PrintingConfig<ExampleClass<int>>>();            
        }
        
        [Test]
        public void Printer_ShouldAddFieldsToSerializingByDefault()
        {
            var testClass = new ExampleClass<int>() { Field = 2 };
            var type = testClass.GetType();
            
            testClass.PrintToString().Should()
                .Be($"{type.Name}{Environment.NewLine}\tField = 2{Environment.NewLine}");
        }
        
        [Test]
        public void Printer_ShouldExcludeTypes()
        {
            var testClass = new ExampleClass<int>() { Field = 2 };
            var type = testClass.GetType();

            var printingConfig = ObjectPrinter.For<ExampleClass<int>>().Excluding<int>();

            printingConfig.PrintToString(testClass).Should().Be(type.Name + Environment.NewLine);
        }

        [Test]
        public void Printer_ShouldHaveAlternativeWayOfSerialization()
        {
            var testClass = new ExampleClass<int>() {Field = 2};
            var type = testClass.GetType();

            var printingConfig = ObjectPrinter.For<ExampleClass<int>>().Printing<int>().Using(p => "TWO");

            printingConfig.PrintToString(testClass)
                .Should().Be($"{type.Name}{Environment.NewLine}\tField = TWO{Environment.NewLine}");
        }

        [Test]
        public void Printer_ShouldChangeCulture()
        {
            var testClass = new ExampleClass<double>() {Field = 2.01};
            var type = testClass.GetType();

            var culture = CultureInfo.GetCultureInfo("en-EN");
            var printingConfig = ObjectPrinter.For<ExampleClass<double>>().Printing<double>().Using(culture);

            printingConfig.PrintToString(testClass)
                .Should().Be($"{type.Name}{Environment.NewLine}\tField = 2.01{Environment.NewLine}");
        }
        
        [Test]
        public void Printer_ShouldChangeSerializationForProperty()
        {
            var testClass = new ExampleClass<string>() {Field = "aaa"};
            var type = testClass.GetType();

            var printingConfig = ObjectPrinter.For<ExampleClass<string>>().Printing<string>().CutToLenght(2);

            printingConfig.PrintToString(testClass)
                .Should().Be($"{type.Name}{Environment.NewLine}\tField = aa{Environment.NewLine}");
        }
        
        [Test]
        public void Printer_ShouldCorrectCuttingStringProperties()
        {
            var testClass = new ExampleClass<string>() {Field = "aaa"};
            var type = testClass.GetType();

            var printingConfig = ObjectPrinter.For<ExampleClass<string>>().Printing<string>().CutToLenght(2);

            printingConfig.PrintToString(testClass)
                .Should().Be($"{type.Name}{Environment.NewLine}\tField = aa{Environment.NewLine}");
        }
    }
}