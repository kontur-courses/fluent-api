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
                "\tDouble = 12.34\r\n"
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
                "\tDouble = 12.34\r\n"
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
                "\tDouble = 12.34\r\n"
            );
    }
}