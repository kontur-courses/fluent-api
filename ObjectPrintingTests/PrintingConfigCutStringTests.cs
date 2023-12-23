using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigCutStringTests
{
    [Test]
    public void SetStringMaxLength_ThrowsArgumentException_WhenProvidedLengthIsNegative()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ObjectPrinter.For<ComplexPerson>()
                .SelectMember(p => p.Name)
                .SetStringMaxLength(-1);
        });
    }

    [Test]
    public async Task SetStringMaxLength_PrintsEntireString_WhenMaxLengthIsGreaterThanValueLength()
    {
        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Name)
            .SetStringMaxLength(100);

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task SetStringMaxLength_PrintsCutString_WhenValueLengthIsLessThanMaxLength()
    {
        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Name)
            .SetStringMaxLength(2);

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }
}