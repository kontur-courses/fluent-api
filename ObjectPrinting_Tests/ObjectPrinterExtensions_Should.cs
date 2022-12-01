using System.Collections;
using System.Globalization;
using ObjectPrinting;
using ObjectPrinting.Abstractions.Configs;

namespace ObjectPrinting_Tests;

[TestFixture]
public class ObjectPrinterExtensions_Should
{
    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.PrimitiveAndFinalTypes))]
    public void ReturnCorrectResult_WithPrimitiveAndFinalTypes<T>(T obj, string expected)
    {
        obj.PrintToString().Should().Be(expected);
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.StringMaxLengthConfiguration))]
    public void ReturnCorrectResult_WithCorrectStringMaxLength(string source, int maxLength, string expected)
    {
        source.PrintToString(cfg => cfg.Printing<string>().WithMaxLength(maxLength))
            .Should().Be(expected);
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.CultureConfiguration))]
    public void ReturnCorrectResult_WithCultureConfiguration<T>(T obj, CultureInfo cultureInfo, string expected)
    {
        obj.PrintToString(cfg => cfg
            .Printing<float>().WithCulture(cultureInfo)
            .Printing<double>().WithCulture(cultureInfo)
            .Printing<DateTime>().WithCulture(cultureInfo)
        ).Should().Be(expected);
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.RoundingConfiguration))]
    public void ReturnCorrectResult_WithRounding<T>(T obj, int decimalPartLength, string expected)
    {
        obj.PrintToString(cfg => cfg
            .Printing<float>().WithRounding(decimalPartLength)
            .Printing<double>().WithRounding(decimalPartLength)
        ).Should().Be(expected);
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.TestObjectPrintConfiguration))]
    public void ReturnCorrectResult_ForUserObjectWithConfiguration<T>(
        Func<T> objProvider,
        Func<IPrintingConfig<T>, IPrintingConfig<T>> cfg,
        string expected
    )
    {
        objProvider().PrintToString(cfg)
            .Should().Be(expected);
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.OneLineEnumerable))]
    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.MultilineEnumerable))]
    public void ReturnCorrectResult_ForEnumerable<T>(T obj, string expected)
        where T : IEnumerable
    {
        obj.PrintToString().Should().Be(expected);
    }

    [TestCaseSource(typeof(ObjectPrinterTestCaseData), nameof(ObjectPrinterTestCaseData.LoopReferences))]
    public void ReturnCorrectResult_LoopReferences<T>(Func<T> objProvider, string expected)
    {
        var result = objProvider().PrintToString();
        result.Should().Be(expected);
        Console.WriteLine(result);
    }
}