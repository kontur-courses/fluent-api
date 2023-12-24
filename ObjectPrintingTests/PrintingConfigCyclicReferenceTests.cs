using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigCyclicReferenceTests
{
    [Test]
    public async Task PrintToString_PrintsCorrectResult_WithDirectCyclicReference()
    {
        TestDataFactory.ComplexPerson.Parent = TestDataFactory.ComplexPerson;
        
        var printer = ObjectPrinter.For<ComplexPerson>();

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task PrintToString_PrintsCorrectResult_WithNestedCyclicReference()
    {
        TestDataFactory.ComplexPerson.Parent = TestDataFactory.Parent;
        TestDataFactory.Parent.Parent = TestDataFactory.ComplexPerson;

        var printer = ObjectPrinter.For<ComplexPerson>();

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task PrintToString_PrintsCorrectResult_WhenCyclicReferenceIsExcluded()
    {
        TestDataFactory.ComplexPerson.Parent = TestDataFactory.ComplexPerson;

        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Parent)
            .ExcludeMember();

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }
    
    [TearDown]
    public void TearDown()
    {
        TestDataFactory.ComplexPerson.Parent = null;
        TestDataFactory.Parent.Parent = null;
    }
}