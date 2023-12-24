using System.Globalization;
using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public async Task AcceptanceTest()
    {
        var person = new Person(new Guid(), "Alex", 192.8, 33);

        var printer = ObjectPrinter.For<Person>()
            .ExcludeType<int>()
            .SetCultureForType<double>(new CultureInfo("ru"))
            .SelectMember(m => m.Name)
            .SetStringMaxLength(2)
            .SelectMember(m => m.Id)
            .WithSerialization(_ => "this is guid");

        await Verify(printer.PrintToString(person));
    }
}