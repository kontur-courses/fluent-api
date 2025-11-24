using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.PrintingConfigs.Extensions;
using Tests.TestEntities;

namespace Tests.Tests;

public partial class ObjectPrintingTests
{
    [TestFixture]
    public class CultureTests
    {
        private CultureTestClass testData;

        [SetUp]
        public void SetUp()
        {
            testData = new CultureTestClass
            {
                Decimal = 1234.56m,
                Date = new DateTime(2023, 10, 15, 14, 30, 0),
                Double = 1234.567
            };
        }
        
        [Test]
        public void PrintToString_CustomCulture_ShouldApplyFormatting()
        {
            var printer = ObjectPrinter.For<CultureTestClass>()
                .Printing<decimal>().Using(new CultureInfo("de-DE"))
                .Create();
            var result = printer.PrintToString(testData);
            
            var expected = $"""
                            CultureTestClass
                            {'\t'}Decimal = 1234,56
                            {'\t'}Date = 10/15/2023 14:30:00
                            {'\t'}Double = 1234.567

                            """; 

            result.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_MultipleCultures_ShouldApplyAll()
        {
            var printer = ObjectPrinter.For<CultureTestClass>()
                .Printing<double>().Using(new CultureInfo("es-ES"))
                .Printing<decimal>().Using(new CultureInfo("en-US"))
                .Printing<DateTime>().Using(new CultureInfo("fr-FR"))
                .Create();

            var result = printer.PrintToString(testData);
            
            var expected = $"""
                            CultureTestClass
                            {'\t'}Decimal = 1234.56
                            {'\t'}Date = 15/10/2023 14:30:00
                            {'\t'}Double = 1234,567

                            """; 
            
            result.Should().Be(expected);
        }
    }
}