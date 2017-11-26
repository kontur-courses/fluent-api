using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting
{

    public class ExampleClass
    {
        public int Field;
    }
    
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [Test]
        public void Printer_ShouldExcludeTypes()
        {
            var testClass = new ExampleClass() { Field = 2 };
            var type = testClass.GetType();
            
            var printingConfig = ObjectPrinter.For<ExampleClass>();

            printingConfig.PrintToString(testClass).Should().Be(type.Name + Environment.NewLine);
        }
        
    }
}