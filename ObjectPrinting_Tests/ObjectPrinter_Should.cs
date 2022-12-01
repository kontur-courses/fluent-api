using System.Collections;
using System.Globalization;
using ObjectPrinting;
using ObjectPrinting.Abstractions.Configs;

namespace ObjectPrinting_Tests;

[TestFixture]
public class ObjectPrinter_Should
{
    [Test]
    public void ReturnCorrectResult_ForNull()
    {
        ObjectPrinter.For<object>().PrintToString(null)
            .Should().Be("null");
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.PrimitiveAndFinalTypes))]
    public void ReturnCorrectResult_WithPrimitiveAndFinalTypes<T>(T obj, string expected)
    {
        ObjectPrinter.For<T>().PrintToString(obj)
            .Should().Be(expected);
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.StringMaxLengthConfiguration))]
    public void ReturnCorrectResult_WithCorrectStringMaxLength(string source, int maxLength, string expected)
    {
        ObjectPrinter.For<string>()
            .Printing<string>().WithMaxLength(maxLength)
            .PrintToString(source)
            .Should().Be(expected);
    }

    [Test]
    public void ThrowArgumentException_OnSetNegativeStringMaxLength()
    {
        ObjectPrinter.For<string>().Printing<string>()
            .Invoking(cfg => cfg.WithMaxLength(-5))
            .Should().Throw<ArgumentException>();
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.CultureConfiguration))]
    public void ReturnCorrectResult_WithCultureConfiguration<T>(T obj, CultureInfo cultureInfo, string expected)
    {
        ObjectPrinter.For<T>()
            .Printing<float>().WithCulture(cultureInfo)
            .Printing<double>().WithCulture(cultureInfo)
            .Printing<DateTime>().WithCulture(cultureInfo)
            .PrintToString(obj)
            .Should().Be(expected);
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.RoundingConfiguration))]
    public void ReturnCorrectResult_WithRounding<T>(T obj, int decimalPartLength, string expected)
    {
        ObjectPrinter.For<T>()
            .Printing<float>().WithRounding(decimalPartLength)
            .Printing<double>().WithRounding(decimalPartLength)
            .PrintToString(obj)
            .Should().Be(expected);
    }

    [TestCase(1f, TestName = "Float")]
    [TestCase(1d, TestName = "Double")]
    public void ThrowArgumentException_OnSetNegativeDecimalPartLength<T>(T instance)
    {
        ObjectPrinter.For<double>().Printing<float>()
            .Invoking(cfg => cfg.WithRounding(-1))
            .Should().Throw<ArgumentException>();
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.TestObjectPrintConfiguration))]
    public void ReturnCorrectResult_ForUserObjectWithConfiguration<T>(
        Func<T> objProvider,
        Func<IPrintingConfig<T>, IPrintingConfig<T>> cfg,
        string expected
    )
    {
        cfg(ObjectPrinter.For<T>())
            .PrintToString(objProvider())
            .Should().Be(expected);
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.OneLineEnumerable))]
    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.MultilineEnumerable))]
    public void ReturnCorrectResult_ForEnumerable<T>(T obj, string expected)
        where T : IEnumerable
    {
        ObjectPrinter.For<T>()
            .PrintToString(obj)
            .Should().Be(expected);
    }


    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.LoopReferences))]
    public void ReturnCorrectResult_LoopReferences<T>(Func<T> objProvider, string expected)
    {
        var result = ObjectPrinter.For<T>().PrintToString(objProvider());
        result.Should().Be(expected);
        Console.WriteLine(result);
    }
}