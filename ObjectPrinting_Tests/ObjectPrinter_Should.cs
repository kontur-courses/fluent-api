using System.Globalization;
using ObjectPrinting;

namespace ObjectPrinting_Tests;

public class ObjectPrinter_Should
{
    private TestObject _defTestObj = null!;

    [SetUp]
    public void SetUp()
    {
        _defTestObj = new TestObject
        {
            Int1 = 10,
            Int2 = 12,
            Double = 12.34,
            Str = "abc"
        };
    }

    [Test]
    public void ReturnCorrectResult_WithoutConfiguration()
    {
        ObjectPrinter.For<TestObject>()
            .PrintToString(_defTestObj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34\r\n" +
                "\tInt2 = 12"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithTypeExcluding()
    {
        ObjectPrinter.For<TestObject>()
            .Exclude<int>()
            .PrintToString(_defTestObj).Should().Be(
                "TestObject\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithMemberExcluding()
    {
        ObjectPrinter.For<TestObject>()
            .Exclude(o => o.Int2)
            .PrintToString(_defTestObj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithTypePrintingConfigured()
    {
        ObjectPrinter.For<TestObject>()
            .Printing<double>().Using(_ => "X")
            .PrintToString(_defTestObj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = X\r\n" +
                "\tInt2 = 12"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithMemberPrintingConfigured()
    {
        ObjectPrinter.For<TestObject>()
            .Printing(o => o.Int1).Using(_ => "X")
            .PrintToString(_defTestObj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = X\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34\r\n" +
                "\tInt2 = 12"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithStringLengthCut()
    {
        ObjectPrinter.For<TestObject>()
            .Printing<string>().WithMaxLength(1)
            .PrintToString(_defTestObj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tStr = a\r\n" +
                "\tDouble = 12.34\r\n" +
                "\tInt2 = 12"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithStringLengthCutLongerThanSource()
    {
        ObjectPrinter.For<TestObject>()
            .Printing<string>().WithMaxLength(10)
            .PrintToString(_defTestObj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34\r\n" +
                "\tInt2 = 12"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithCulture()
    {
        ObjectPrinter.For<TestObject>()
            .Printing<double>().WithCulture(CultureInfo.GetCultureInfoByIetfLanguageTag("Ru"))
            .PrintToString(_defTestObj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12,34\r\n" +
                "\tInt2 = 12"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithRounding()
    {
        ObjectPrinter.For<TestObject>()
            .Printing<double>().WithRounding(1)
            .PrintToString(_defTestObj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.3\r\n" +
                "\tInt2 = 12"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithOneLineValuesArray()
    {
        ObjectPrinter.For<int[]>().PrintToString(new[] {1, 2, 3})
            .Should().Be("{1, 2, 3}");
    }

    [Test]
    public void ReturnCorrectResult_WithMultilineValuesArray()
    {
        ObjectPrinter.For<TestObject[]>().PrintToString(new[]
            {
                _defTestObj,
                _defTestObj
            })
            .Should().Be("{\r\n" +
                         "\tTestObject\r\n" +
                         "\t\tInt1 = 10\r\n" +
                         "\t\tStr = abc\r\n" +
                         "\t\tDouble = 12.34\r\n" +
                         "\t\tInt2 = 12\r\n" +
                         "\tTestObject\r\n" +
                         "\t\tInt1 = 10\r\n" +
                         "\t\tStr = abc\r\n" +
                         "\t\tDouble = 12.34\r\n" +
                         "\t\tInt2 = 12\r\n" +
                         "}");
    }

    [Test]
    public void ReturnCorrectResult_WithDictionary()
    {
        ObjectPrinter.For<Dictionary<int, string>>()
            .PrintToString(new Dictionary<int, string> {{1, "a"}, {2, "b"}, {3, "c"}})
            .Should().Be("{[1, a], [2, b], [3, c]}");
    }

    [Test]
    public void ReturnCorrectResult_LoopReferences()
    {
        var cyclic1 = new CyclicTestObject {Str = "abc"};
        var cyclic2 = new CyclicTestObject {Str = "def", TestObject = cyclic1};
        cyclic1.TestObject = cyclic2;

        ObjectPrinter.For<CyclicTestObject>()
            .PrintToString(cyclic1)
            .Should().Be("CyclicTestObject\r\n" +
                         "\tStr = abc\r\n" +
                         "\tTestObject = CyclicTestObject\r\n" +
                         "\t\tStr = def\r\n" +
                         "\t\tTestObject = [Loop reference]");
    }
}