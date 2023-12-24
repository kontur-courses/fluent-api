using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests;

public class PrintingConfigMemberSerializationTests
{
    [Test]
    public async Task WithSerialization_ReturnsCorrectResult_WhenAlternativeSerializationIsSpecified()
    {
        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Age)
            .WithSerialization(_ => "123");

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task WithSerialization_OverwritesPreviousSerialization_WhenNewSerializationIsProvided()
    {
        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Age)
            .WithSerialization(_ => "123")
            .SelectMember(p => p.Age)
            .WithSerialization(_ => "321");

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task WithSerialization_ConsidersFullMemberName_WhenMemberFullNamesDiffer()
    {
        TestDataFactory.ComplexPerson.Parent = TestDataFactory.Parent;
        
        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Parent!.Age)
            .WithSerialization(_ => "123");

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task WithSerialization_TakesPrecedence_WhenTypeSerializationIsAlsoSpecified()
    {
        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Age)
            .WithSerialization(_ => "123")
            .WithSerializationForType<int>(_ => "321");

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task WithSerializtion_ReturnsCorrectResult_WithComplexTypeMemberSerialization()
    {
        TestDataFactory.ComplexPerson.Parent = TestDataFactory.Parent;

        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Parent)
            .WithSerialization(_ => "custom");

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }

    [Test]
    public async Task WithSerializtion_ReturnsCorrectResult_WhenSerializationIsSetForMultipleMembers()
    {
        var printer = ObjectPrinter.For<ComplexPerson>()
            .SelectMember(p => p.Age)
            .WithSerialization(_ => "123")
            .SelectMember(p => p.Name)
            .WithSerialization(_ => "123");

        await Verify(printer.PrintToString(TestDataFactory.ComplexPerson));
    }
    
    [TearDown]
    public void TearDown()
    {
        TestDataFactory.ComplexPerson.Parent = null;
    }
}