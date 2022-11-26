using ObjectPrinting;

namespace ObjectPrinting_Tests;

public class ObjectPrinter_Should
{
    [Test]
    public void ReturnCorrectResult_WithoutConfiguration()
    {
        var obj = new TestObject(10, 12, "abc", 12.34);
        ObjectPrinter.For<TestObject>()
            .PrintToString(obj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tInt2 = 12\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithTypeExcluding()
    {
        var obj = new TestObject(10, 12, "abc", 12.34);
        ObjectPrinter.For<TestObject>()
            .Exclude<int>()
            .PrintToString(obj).Should().Be(
                "TestObject\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithMemberExcluding()
    {
        var obj = new TestObject(10, 12, "abc", 12.34);
        ObjectPrinter.For<TestObject>()
            .Exclude(o => o.Int2)
            .PrintToString(obj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithTypePrintingConfigured()
    {
        var obj = new TestObject(10, 12, "abc", 12.34);
        ObjectPrinter.For<TestObject>()
            .Printing<double>().Using(_ => "X")
            .PrintToString(obj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tInt2 = 12\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = X"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithMemberPrintingConfigured()
    {
        var obj = new TestObject(10, 12, "abc", 12.34);
        ObjectPrinter.For<TestObject>()
            .Printing(o => o.Int1).Using(_ => "X")
            .PrintToString(obj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = X\r\n" +
                "\tInt2 = 12\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithStringLengthCut()
    {
        var obj = new TestObject(10, 12, "abc", 12.34);
        ObjectPrinter.For<TestObject>()
            .Printing<string>().WithMaxLength(1)
            .PrintToString(obj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tInt2 = 12\r\n" +
                "\tStr = a\r\n" +
                "\tDouble = 12.34"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithStringLengthCutLongerThanSource()
    {
        var obj = new TestObject(10, 12, "abc", 12.34);
        ObjectPrinter.For<TestObject>()
            .Printing<string>().WithMaxLength(10)
            .PrintToString(obj).Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tInt2 = 12\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34"
            );
    }

    [Test]
    public void ReturnCorrectResult_WithFinalValuesArray()
    {
        ObjectPrinter.For<int[]>().PrintToString(new[] {1, 2, 3})
            .Should().Be("[1, 2, 3]");
    }

    [Test]
    public void ReturnCorrectResult_WithNotFinalValuesArray()
    {
        ObjectPrinter.For<TestObject[]>().PrintToString(new[]
            {
                new TestObject(10, 12, "abc", 12.34),
                new TestObject(10, 12, "abc", 12.34)
            })
            .Should().Be("[\r\n" +
                         "\tTestObject\r\n" +
                         "\t\tInt1 = 10\r\n" +
                         "\t\tInt2 = 12\r\n" +
                         "\t\tStr = abc\r\n" +
                         "\t\tDouble = 12.34\r\n" +
                         "\tTestObject\r\n" +
                         "\t\tInt1 = 10\r\n" +
                         "\t\tInt2 = 12\r\n" +
                         "\t\tStr = abc\r\n" +
                         "\t\tDouble = 12.34\r\n" +
                         "]");
    }

    [Test]
    public void ReturnCorrectResult_WithDictionary()
    {
        ObjectPrinter.For<Dictionary<int, string>>()
            .PrintToString(new Dictionary<int, string> {{1, "a"}, {2, "b"}, {3, "c"}})
            .Should().Be("[1, 2, 3]");
    }
}