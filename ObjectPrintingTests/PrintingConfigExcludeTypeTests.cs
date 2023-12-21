using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigExcludeTypeTests
{
    [Test]
    public async Task PrintToString_ReturnsCorrectResult_WithExcludedFinalType()
    {
        var printer = ObjectPrinter.For<Person>()
            .ExcludeType<int>();

        await Verify(printer.PrintToString(TestDataFactory.Person));
    }

    [Test]
    public async Task PrintToString_ReturnsCorrectResult_WithExcludedComplexTypeItself()
    {
        var printer = ObjectPrinter.For<Person>()
            .ExcludeType<Person>();
        
        await Verify(printer.PrintToString(TestDataFactory.Person));
    }

    [Test]
    public async Task PrintToString_ReturnsCorrectResult_WhenExcludingForFinalType()
    {
        var printer = ObjectPrinter.For<int>()
            .ExcludeType<int>();
        
        await Verify(printer.PrintToString(3));
    }

    [Test]
    public async Task PrintToString_DoesNotAffectResult_WhenExcludingNonPresentType()
    {
        var printer = ObjectPrinter.For<Person>()
            .ExcludeType<float>();
        
        await Verify(printer.PrintToString(TestDataFactory.Person));
    }
}