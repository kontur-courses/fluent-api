using System.Globalization;
using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigCultureTests
{
    [Test]
    public async Task SetCultureInfoForType_MakesFloatingPointTypesDisplayComma_WhenCultureInfoIsRu()
    {
        var ruCulture = new CultureInfo("ru");

        var printer = ObjectPrinter.For<Person>()
            .SetCultureForType<double>(ruCulture);

        await Verify(printer.PrintToString(TestDataFactory.Person));
    }

    [Test]
    public async Task SetCultureInfoForType_OverwritesPreviousCulture_WhenMoreThanOneCultureSpecified()
    {
        var ruCulture = new CultureInfo("ru");
        var enCulture = new CultureInfo("en");

        var printer = ObjectPrinter.For<Person>()
            .SetCultureForType<double>(ruCulture)
            .SetCultureForType<double>(enCulture);

        await Verify(printer.PrintToString(TestDataFactory.Person));
    }
}