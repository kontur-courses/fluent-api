using System.Globalization;
using ObjectPrinting;

namespace ObjectPrinting_Tests;

[TestFixture, Parallelizable]
public class ObjectPrinter_Should
{
    private static TestObject CreateTestObject() =>
        new()
        {
            Int1 = 10,
            Int2 = 12,
            Double = 12.34,
            Str = "abc"
        };

    [Test]
    public void ReturnCorrectResult_WithoutConfiguration()
    {
        ObjectPrinter.For<TestObject>()
            .PrintToString(CreateTestObject()).Should().Be(
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
            .PrintToString(CreateTestObject()).Should().Be(
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
            .PrintToString(CreateTestObject()).Should().Be(
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
            .PrintToString(CreateTestObject()).Should().Be(
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
            .PrintToString(CreateTestObject()).Should().Be(
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
            .PrintToString(CreateTestObject()).Should().Be(
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
            .PrintToString(CreateTestObject()).Should().Be(
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
            .PrintToString(CreateTestObject()).Should().Be(
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
            .PrintToString(CreateTestObject()).Should().Be(
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
            .Should().Be("[1, 2, 3]");
    }

    [Test]
    public void ReturnCorrectResult_WithMultilineValuesArray()
    {
        ObjectPrinter.For<TestObject[]>().PrintToString(new[]
            {
                CreateTestObject(),
                CreateTestObject()
            })
            .Should().Be("[\r\n" +
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
                         "]");
    }

    [Test]
    public void ReturnCorrectResult_WithNestedMultilineValuesArray()
    {
        ObjectPrinter.For<TestObject[][]>().PrintToString(new[]
            {
                new[]
                {
                    CreateTestObject(),
                    CreateTestObject()
                }
            })
            .Should().Be("[\r\n" +
                         "\t[\r\n" +
                         "\t\tTestObject\r\n" +
                         "\t\t\tInt1 = 10\r\n" +
                         "\t\t\tStr = abc\r\n" +
                         "\t\t\tDouble = 12.34\r\n" +
                         "\t\t\tInt2 = 12\r\n" +
                         "\t\tTestObject\r\n" +
                         "\t\t\tInt1 = 10\r\n" +
                         "\t\t\tStr = abc\r\n" +
                         "\t\t\tDouble = 12.34\r\n" +
                         "\t\t\tInt2 = 12\r\n" +
                         "\t]\r\n" +
                         "]");
    }

    [Test]
    public void ReturnCorrectResult_WithOneLineDictionaryValues()
    {
        var obj = new Dictionary<TestObject, TestObject>
        {
            {
                CreateTestObject(),
                CreateTestObject()
            }
        };
        ObjectPrinter.For<Dictionary<TestObject, TestObject>>().PrintToString(obj)
            .Should().Be("[\r\n" +
                         "\t{\r\n" +
                         "\t\tTestObject\r\n" +
                         "\t\t\tInt1 = 10\r\n" +
                         "\t\t\tStr = abc\r\n" +
                         "\t\t\tDouble = 12.34\r\n" +
                         "\t\t\tInt2 = 12:\r\n" +
                         "\t\tTestObject\r\n" +
                         "\t\t\tInt1 = 10\r\n" +
                         "\t\t\tStr = abc\r\n" +
                         "\t\t\tDouble = 12.34\r\n" +
                         "\t\t\tInt2 = 12\r\n" +
                         "\t}\r\n" +
                         "]");
    }

    [Test]
    public void ReturnCorrectResult_WithMultilineDictionaryValues()
    {
        ObjectPrinter.For<Dictionary<int, string>>()
            .PrintToString(new Dictionary<int, string> {{1, "a"}, {2, "b"}, {3, "c"}})
            .Should().Be("[{1: a}, {2: b}, {3: c}]");
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