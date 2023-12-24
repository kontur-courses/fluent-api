using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigSelectMemberTests
{
    [Test]
    public void SelectMember_ThrowsMissingMemberException_WhenInvalidMemberIsSpecified()
    {
        Assert.Throws<MissingMemberException>(() =>
        {
            ObjectPrinter.For<ComplexPerson>()
                .SelectMember(p => "123");
        });
    }
}