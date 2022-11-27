using ObjectPrinting;

namespace ObjectPrinting_Tests;

public class ObjectPrinterExtensions_Should
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
    public void ReturnDefaultPrint_WithoutConfig()
    {
        _defTestObj.PrintToString()
            .Should().Be(
                "TestObject\r\n" +
                "\tInt1 = 10\r\n" +
                "\tStr = abc\r\n" +
                "\tDouble = 12.34\r\n" +
                "\tInt2 = 12"
            );
    }

    [Test]
    public void ReturnCorrectPrint_WithConfig()
    {
        _defTestObj.PrintToString(cfg => cfg
            .Exclude<int>()
            .Printing(obj => obj.Double).Using(_ => "X")
            .Printing<string>().WithMaxLength(2)
        ).Should().Be(
            "TestObject\r\n" +
            "\tStr = ab\r\n" +
            "\tDouble = X"
        );
    }
}