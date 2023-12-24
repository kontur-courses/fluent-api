using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigTypeSerializationTests
{
    [Test]
    public async Task WithSerializationForType_ReturnsCorrectResult_WhenAlternativeFinalTypeSerializationIsSpecified()
    {
        var printer = ObjectPrinter.For<Person>()
            .WithSerializationForType<string>(_ => "Misha");

        await Verify(printer.PrintToString(TestDataFactory.Person));
    }

    [Test]
    public async Task WithSerializationForType_ReturnsCorrectResult_WhenAlternativeComplexTypeSerializationIsSpecified()
    {
        var printer = ObjectPrinter.For<Person>()
            .WithSerializationForType<Person>(_ => "This is a \"Person\" type");

        await Verify(printer.PrintToString(TestDataFactory.Person));
    }
    
    [Test]
    public async Task WithSerializationForType_OverwritesPreviousSerialization_WhenNewSerializationIsProvided()
    {
        var printer = ObjectPrinter.For<Person>()
            .WithSerializationForType<string>(_ => "first")
            .WithSerializationForType<string>(_ => "second");

        await Verify(printer.PrintToString(TestDataFactory.Person));
    }

    [Test]
    public async Task WithSerializationForType_ReturnsCorrectResult_WhenUsingPropertiesOfComplexType()
    {
        var printer = ObjectPrinter.For<Person>()
            .WithSerializationForType<Person>(p => p.Name + " " + p.Age);

        await Verify(printer.PrintToString(TestDataFactory.Person));
    }
}