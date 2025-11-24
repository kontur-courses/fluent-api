using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.PrintingConfigs.Extensions;
using Tests.TestEntities;

namespace Tests.Tests;

public partial class ObjectPrintingTests
{
    [TestFixture]
    public class CustomSerializerTests
    {
        private CustomSerializerTestsClass testData;

        [SetUp]
        public void SetUp()
        {
            testData = new CustomSerializerTestsClass
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Name = "John",
                Age = 25,
                Price = 19.99m,
                Discount = 0.1m
            };
        }

        [Test]
        public void PrintToString_CustomTypeSerializer_ShouldFormatType()
        {
            var printer = ObjectPrinter.For<CustomSerializerTestsClass>()
                .Printing<Guid>().Using(g => g
                    .ToString("N").Substring(0, 8).ToUpper())
                .Create();

            var result = printer.PrintToString(testData);
            
            var expected = $"""
                            CustomSerializerTestsClass
                            {'\t'}Id = A1B2C3D4
                            {'\t'}Name = John
                            {'\t'}Age = 25
                            {'\t'}Price = 19.99
                            {'\t'}Discount = 0.1

                            """; 

            
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_CustomPropertySerializer_ShouldOverrideTypeSerializer()
        {
            var printer = ObjectPrinter.For<CustomSerializerTestsClass>()
                .Printing<int>().Using(i => $"{i} years")
                .Printing(p => p.Age).Using(age => $"***{age}***")
                .Create();

            var result = printer.PrintToString(testData);
            
            var expected = $"""
                            CustomSerializerTestsClass
                            {'\t'}Id = a1b2c3d4-e5f6-7890-abcd-ef1234567890
                            {'\t'}Name = John
                            {'\t'}Age = ***25***
                            {'\t'}Price = 19.99
                            {'\t'}Discount = 0.1

                            """; 
            
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_CustomSerializerReturnsNull_ShouldHandleCorrectly()
        {
            var printer = ObjectPrinter.For<CustomSerializerTestsClass>()
                .Printing(p => p.Name).Using(_ => null)
                .Create();

            var result = printer.PrintToString(testData);
            
            var expected = $"""
                            CustomSerializerTestsClass
                            {'\t'}Id = a1b2c3d4-e5f6-7890-abcd-ef1234567890
                            {'\t'}Name = 
                            {'\t'}Age = 25
                            {'\t'}Price = 19.99
                            {'\t'}Discount = 0.1

                            """; 
            
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_CustomSerializerForMultipleTypes_ShouldApplyAll()
        {
            var printer = ObjectPrinter.For<CustomSerializerTestsClass>()
                .Printing<Guid>().Using(g => g.ToString().Substring(0, 8))
                .Printing<int>().Using(i => $"#{i}")
                .Printing<decimal>().Using(p => $"${p}")
                .Create();

            var result = printer.PrintToString(testData);

            var expected = $"""
                            CustomSerializerTestsClass
                            {'\t'}Id = a1b2c3d4
                            {'\t'}Name = John
                            {'\t'}Age = #25
                            {'\t'}Price = $19,99
                            {'\t'}Discount = $0,1

                            """; 
            
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_CustomSerializerForOneProperty_ShouldNotAffectOtherProperties()
        {
            var printer = ObjectPrinter.For<CustomSerializerTestsClass>()
                .Printing(p => p.Price).Using(p => $"${p}")
                .Create();

            var result = printer.PrintToString(testData);

            var expected = $"""
                            CustomSerializerTestsClass
                            {'\t'}Id = a1b2c3d4-e5f6-7890-abcd-ef1234567890
                            {'\t'}Name = John
                            {'\t'}Age = 25
                            {'\t'}Price = $19,99
                            {'\t'}Discount = 0.1

                            """; 
            
            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_MultiplePropertySerializers_ShouldApplyEachToRespectiveProperty()
        {
            var printer = ObjectPrinter.For<CustomSerializerTestsClass>()
                .Printing(p => p.Name).Using(n => $"{n.ToUpper()}")
                .Printing(p => p.Age).Using(a => $"{a} years")
                .Printing(p => p.Price).Using(p => $"${p}")
                .Create();

            var result = printer.PrintToString(testData);
            
            
            var expected = $"""
                            CustomSerializerTestsClass
                            {'\t'}Id = a1b2c3d4-e5f6-7890-abcd-ef1234567890
                            {'\t'}Name = JOHN
                            {'\t'}Age = 25 years
                            {'\t'}Price = $19,99
                            {'\t'}Discount = 0.1

                            """; 
            
            result.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_CustomSerializerDominatesOverCulture()
        {
            var printer = ObjectPrinter.For<CustomSerializerTestsClass>()
                .Printing<decimal>().Using(new CultureInfo("de-DE"))
                .Printing<decimal>().Using(p => $"${p:0.00}")
                .Create();

            var result = printer.PrintToString(testData);
            
            var expected = $"""
                            CustomSerializerTestsClass
                            {'\t'}Id = a1b2c3d4-e5f6-7890-abcd-ef1234567890
                            {'\t'}Name = John
                            {'\t'}Age = 25
                            {'\t'}Price = $19,99
                            {'\t'}Discount = $0,10

                            """; 
            
            result.Should().Be(expected);
        }
    }
}