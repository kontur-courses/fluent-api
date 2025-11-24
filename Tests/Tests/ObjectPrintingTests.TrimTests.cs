using FluentAssertions;
using ObjectPrinting;
using Tests.TestEntities;

namespace Tests.Tests;

public partial class ObjectPrintingTests
{
    [TestFixture]
    public class TrimTests
    {
        [Test]
        public void PrintToString_TrimToZeroLength_ShouldReturnEmptyString()
        {
            var data = new TrimTestsClass { Long = "Very long text" };
            var printer = ObjectPrinter.For<TrimTestsClass>()
                .Printing(p => p.Long).TrimToLength(0)
                .Create();
            var result = printer.PrintToString(data);
            
            var expected = $"""
                            TrimTestsClass
                            {'\t'}Short = null
                            {'\t'}Long = 
                            {'\t'}Description = null

                            """;
            
            result.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_TrimMultipleStringProperties_ShouldTrimEachIndependently()
        {
            var data = new TrimTestsClass
            {
                Short = "Short",
                Long = "Very long text here",
                Description = "Medium description"
            };
            var printer = ObjectPrinter.For<TrimTestsClass>()
                .Printing(p => p.Short).TrimToLength(3)
                .Printing(p => p.Long).TrimToLength(10)
                .Printing(p => p.Description).TrimToLength(6)
                .Create();
            var result = printer.PrintToString(data);
            
            var expected = $"""
                            TrimTestsClass
                            {'\t'}Short = Sho
                            {'\t'}Long = Very long 
                            {'\t'}Description = Medium

                            """;

            result.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_TrimToLengthGreaterThanString_ShouldReturnOriginalString()
        {
            var data = new TrimTestsClass { Short = "Hi" };
            var printer = ObjectPrinter.For<TrimTestsClass>()
                .Printing(p => p.Short).TrimToLength(10)
                .Create(); 
            var result = printer.PrintToString(data);
            
            var expected = $"""
                            TrimTestsClass
                            {'\t'}Short = Hi
                            {'\t'}Long = null
                            {'\t'}Description = null

                            """;

            result.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_TrimNullString_ShouldHandleGracefully()
        {
            var data = new TrimTestsClass { Short = null };
            var printer = ObjectPrinter.For<TrimTestsClass>()
                .Printing(p => p.Short).TrimToLength(5)
                .Create();
            var result = printer.PrintToString(data);
            
            var expected = $"""
                            TrimTestsClass
                            {'\t'}Short = null
                            {'\t'}Long = null
                            {'\t'}Description = null

                            """;

            result.Should().Be(expected);
        }
        
        [Test]
        public void TrimToLength_NotPositiveValue_ShouldThrowArgumentException()
        {
            var printer = ObjectPrinter.For<TrimTestsClass>();
            
            var act = () => printer.Printing(p => p.Short).TrimToLength(-1);
            
            act.Should().Throw<ArgumentException>();
        }
    }
}

