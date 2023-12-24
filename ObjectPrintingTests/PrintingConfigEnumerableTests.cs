using System.Globalization;
using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigEnumerableTests
{
    [Test]
    public async Task PrintToString_PrintsEmptyLine_WhenCollectionIsExcluded()
    {
        var reader = ObjectPrinter.For<EnumerableTest>()
            .ExcludeType<List<ComplexPerson>>();

        await Verify(reader.PrintToString(TestDataFactory.EnumerableSimpleTest));
    }

    [Test]
    public async Task PrintToString_PrintsEmptyLine_WhenTypeOfCollectionElementsIsExcluded()
    {
        var reader = ObjectPrinter.For<EnumerableTest>()
            .ExcludeType<ComplexPerson>();

        await Verify(reader.PrintToString(TestDataFactory.EnumerableSimpleTest));
    }

    [Test]
    public async Task PrintToString_PrintsCorrectResult_WhenCollectionIsEmpty()
    {
        var reader = ObjectPrinter.For<EnumerableTest>();

        await Verify(reader.PrintToString(TestDataFactory.EnumerableEmptyTest));
    }

    [Test]
    public async Task PrintToString_AppliesConfigToCollectionElements_WhenElementsMatchConfig()
    {
        var reader = ObjectPrinter.For<EnumerableTest>()
            .SetCultureForType<double>(new CultureInfo("ru"))
            .WithSerializationForType<Dictionary<int, DateTime>>(_ => "this is a dictionary object")
            .WithSerializationForType<ComplexPerson>(_ => "ComplexPerson object");

        await Verify(reader.PrintToString(TestDataFactory.EnumerableSimpleTest));
    }

    [Test]
    public async Task PrintToString_PrintsCorrectResult_WithCollectionElementsWithCyclicReference()
    {
        TestDataFactory.EnumerableSimpleTest.List[0].Parent = TestDataFactory.EnumerableSimpleTest.List[0];

        var reader = ObjectPrinter.For<EnumerableTest>();

        await Verify(reader.PrintToString(TestDataFactory.EnumerableSimpleTest));
    }

    [TearDown]
    public void TearDown()
    {
        TestDataFactory.EnumerableSimpleTest.List[0].Parent = null;
    }
}