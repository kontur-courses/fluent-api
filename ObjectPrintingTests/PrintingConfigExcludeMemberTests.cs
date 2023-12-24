using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigExcludeMemberTests
{
    [Test]
    public async Task ExcludeMember_ReturnsCorrectResult_WithExcludedMemberOfFinalType()
    {
        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Age)
            .ExcludeMember();

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task ExcludeMember_ExcludesBasedOnFullName_WhenMoreThanOneMemberWithSameNameIsPresent()
    {
        TestDataFactory.ComplexPerson.Parent = TestDataFactory.Parent;

        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Parent!.Age)
            .ExcludeMember();

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task ExcludeMember_ReturnsCorrectResult_WhenExcludingComplexType()
    {
        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Parent)
            .ExcludeMember();

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [TearDown]
    public void TearDown()
    {
        TestDataFactory.ComplexPerson.Parent = null;
    }
}