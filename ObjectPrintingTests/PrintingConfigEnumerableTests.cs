using System.Globalization;
using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigEnumerableTests
{
    [Test]
    public async Task PrintToString_PrintsEmptyLine_WhenCollectionIsExcluded()
    {
        var printer = ObjectPrinter.For<EnumerableTest>()
            .ExcludeType<List<ComplexPerson>>();

        await Verify(printer.PrintToString(TestDataFactory.EnumerableSimpleTest));
    }

    [Test]
    public async Task PrintToString_PrintsEmptyLine_WhenTypeOfCollectionElementsIsExcluded()
    {
        var printer = ObjectPrinter.For<EnumerableTest>()
            .ExcludeType<ComplexPerson>();

        await Verify(printer.PrintToString(TestDataFactory.EnumerableSimpleTest));
    }

    [Test]
    public async Task PrintToString_PrintsCorrectResult_WhenCollectionIsEmpty()
    {
        var printer = ObjectPrinter.For<EnumerableTest>();

        await Verify(printer.PrintToString(TestDataFactory.EnumerableEmptyTest));
    }

    [Test]
    public async Task PrintToString_AppliesConfigToCollectionElements_WhenElementsMatchConfig()
    {
        var printer = ObjectPrinter.For<EnumerableTest>()
            .SetCultureForType<double>(new CultureInfo("ru"))
            .WithSerializationForType<Dictionary<int, DateTime>>(_ => "this is a dictionary object")
            .WithSerializationForType<ComplexPerson>(_ => "ComplexPerson object");

        await Verify(printer.PrintToString(TestDataFactory.EnumerableSimpleTest));
    }

    [Test]
    public async Task PrintToString_PrintsCorrectResult_WithCollectionElementsWithCyclicReference()
    {
        TestDataFactory.EnumerableSimpleTest.List[0].Parent = TestDataFactory.EnumerableSimpleTest.List[0];

        var printer = ObjectPrinter.For<EnumerableTest>();

        await Verify(printer.PrintToString(TestDataFactory.EnumerableSimpleTest));
    }

    [Test]
    public async Task PrintToString_PrintsCorrectResult_WithNestedCollections()
    {
        var printer = ObjectPrinter.For<ComplexEnumerable>();

        await Verify(printer.PrintToString(TestDataFactory.ComplexEnumerable));
    }

    [Test]
    public async Task PrintToString_AppliesConfigToMembersOfIndexedElementsOnly_WhenSpecified()
    {
        var printer = ObjectPrinter.For<ComplexEnumerable>()
            .SelectMember(e => e.Persons[0].Name)
            .WithSerialization(_ => "123123")
            .SelectMember(e => e.Dictionary["asd"].Age)
            .WithSerialization(_ => "age");

        await Verify(printer.PrintToString(TestDataFactory.ComplexEnumerable));
    }
    
    [TearDown]
    public void TearDown()
    {
        TestDataFactory.EnumerableSimpleTest.List[0].Parent = null;
    }
}