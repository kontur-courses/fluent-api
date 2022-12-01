using System.Globalization;
using ObjectPrinting.Abstractions.Configs;

namespace ObjectPrinting_Tests;

public static class ObjectPrinterTestCaseData
{
    public static readonly TestCaseData[] PrimitiveAndFinalTypes =
    {
        new TestCaseData(true, "True").SetName("Boolean"),
        new TestCaseData((byte) 1, "1").SetName("Byte"),
        new TestCaseData((sbyte) -5, "-5").SetName("Signed byte"),
        new TestCaseData((short) -5000, "-5000").SetName("Short"),
        new TestCaseData((ushort) 5000, "5000").SetName("Unsigned short"),
        new TestCaseData(-1000000, "-1000000").SetName("Int"),
        new TestCaseData(1000000, "1000000").SetName("Unsigned int"),
        new TestCaseData(IntPtr.Zero, "0").SetName("IntPtr"),
        new TestCaseData(UIntPtr.Zero, "0").SetName("Unsigned intPtr"),
        new TestCaseData('x', "x").SetName("Char"),
        new TestCaseData(10.54f, "10.54").SetName("Float"),
        new TestCaseData(10.54d, "10.54").SetName("Double"),
        new TestCaseData("abc", "abc").SetName("String"),
        new TestCaseData(new Guid(), "00000000-0000-0000-0000-000000000000").SetName("Guid"),
        new TestCaseData(new DateTime(2000, 1, 1), "01/01/2000 00:00:00").SetName("DateTime"),
        new TestCaseData(new TimeSpan(2, 0, 0), "02:00:00").SetName("TimeSpan"),
        new TestCaseData((1, "x"), "(1, x)").SetName("Tuple with two values"),
        new TestCaseData((1, "x", 1.5, 'z'), "(1, x, 1.5, z)").SetName("Tuple with many values")
    };

    public static readonly TestCaseData[] StringMaxLengthConfiguration =
    {
        new TestCaseData("ab c d ef", 3, "ab ").SetName("Cut if longer than max"),
        new TestCaseData("abc", 50, "abc").SetName("Do nothing if shorter than max")
    };

    public static readonly TestCaseData[] CultureConfiguration =
    {
        new TestCaseData(12.34f, CultureInfo.GetCultureInfoByIetfLanguageTag("ru"), "12,34").SetName("Float with ru"),
        new TestCaseData(12.34f, CultureInfo.GetCultureInfoByIetfLanguageTag("us"), "12.34").SetName("Float with us"),
        new TestCaseData(12.34, CultureInfo.GetCultureInfoByIetfLanguageTag("ru"), "12,34").SetName("Double with ru"),
        new TestCaseData(12.34, CultureInfo.GetCultureInfoByIetfLanguageTag("us"), "12.34").SetName("Double with us"),
        new TestCaseData(new DateTime(2000, 1, 1), CultureInfo.CreateSpecificCulture("ru"), "01.01.2000 00:00:00")
            .SetName("Date time with ru"),
        new TestCaseData(new DateTime(2000, 1, 1), CultureInfo.CreateSpecificCulture("us"), "01/01/2000 00:00:00")
            .SetName("Date time with us"),
    };

    public static readonly TestCaseData[] RoundingConfiguration =
    {
        new TestCaseData(12.3456f, 2, "12.35").SetName("Float rounding"),
        new TestCaseData(12.3456, 2, "12.35").SetName("Double rounding"),
        new TestCaseData(12.34f, 5, "12.34").SetName("Float rounding with max longer than actual"),
        new TestCaseData(12.34, 5, "12.34").SetName("Double rounding with max longer than actual"),
    };

    public static readonly TestCaseData[] TestObjectPrintConfiguration =
    {
        new TestCaseData(
            CreateTestObject,
            new Func<IPrintingConfig<TestObject>, IPrintingConfig<TestObject>>(cfg => cfg),
            "TestObject\r\n" +
            "\tInt1 = 10\r\n" +
            "\tStr = abc\r\n" +
            "\tInt2 = 12"
        ).SetName("No configuration"),
        new TestCaseData(
            () => new TestObject {Str = null, Int1 = 10, Int2 = 12},
            new Func<IPrintingConfig<TestObject>, IPrintingConfig<TestObject>>(cfg => cfg),
            "TestObject\r\n" +
            "\tInt1 = 10\r\n" +
            "\tStr = null\r\n" +
            "\tInt2 = 12"
        ).SetName("No configuration, null field"),
        new TestCaseData(
            CreateTestObject,
            new Func<IPrintingConfig<TestObject>, IPrintingConfig<TestObject>>(cfg => cfg.Exclude<int>()),
            "TestObject\r\n" +
            "\tStr = abc"
        ).SetName("Type excluding"),
        new TestCaseData(
            CreateTestObject,
            new Func<IPrintingConfig<TestObject>, IPrintingConfig<TestObject>>(cfg => cfg.Exclude(o => o.Int2)),
            "TestObject\r\n" +
            "\tInt1 = 10\r\n" +
            "\tStr = abc"
        ).SetName("Member excluding"),
        new TestCaseData(
            CreateTestObject,
            new Func<IPrintingConfig<TestObject>, IPrintingConfig<TestObject>>(cfg =>
                cfg.Printing<string>().Using(s => s.ToUpper())
            ),
            "TestObject\r\n" +
            "\tInt1 = 10\r\n" +
            "\tStr = ABC\r\n" +
            "\tInt2 = 12"
        ).SetName("Custom type print"),
        new TestCaseData(
            CreateTestObject,
            new Func<IPrintingConfig<TestObject>, IPrintingConfig<TestObject>>(cfg =>
                cfg.Printing(o => o.Int1).Using(i => (i * 10).ToString())
            ),
            "TestObject\r\n" +
            "\tInt1 = 100\r\n" +
            "\tStr = abc\r\n" +
            "\tInt2 = 12"
        ).SetName("Custom member print"),
    };


    public static readonly TestCaseData[] OneLineEnumerable =
    {
        new TestCaseData(new[] {1, 2, 3}, "[1, 2, 3]").SetName("Int array"),
        new TestCaseData(new[] {"a", "b", "c"}, "[a, b, c]").SetName("String array"),
        new TestCaseData(new object[] {1, "b", 143.45}, "[1, b, 143.45]").SetName("Object array"),
        new TestCaseData(new List<char> {'1', '2', '3'}, "[1, 2, 3]").SetName("List"),
        new TestCaseData(new List<double> {1, 2.3, 45.5}.AsEnumerable(), "[1, 2.3, 45.5]").SetName("Enumerable"),
        new TestCaseData(new Dictionary<int, string> {{1, "a"}, {2, "b"}, {3, "c"}}, "[{1: a}, {2: b}, {3: c}]")
            .SetName("Dictionary"),
        new TestCaseData(new[] {new[] {1, 2}, new[] {3, 4}}, "[[1, 2], [3, 4]]").SetName("Nested array"),
    };

    public static readonly TestCaseData[] MultilineEnumerable =
    {
        new TestCaseData(
            new[]
            {
                CreateTestObject(),
                CreateTestObject()
            },
            "[\r\n" +
            "\tTestObject\r\n" +
            "\t\tInt1 = 10\r\n" +
            "\t\tStr = abc\r\n" +
            "\t\tInt2 = 12\r\n" +
            "\tTestObject\r\n" +
            "\t\tInt1 = 10\r\n" +
            "\t\tStr = abc\r\n" +
            "\t\tInt2 = 12\r\n" +
            "]"
        ).SetName("Multiline array"),
        new TestCaseData(
            new[]
            {
                new[]
                {
                    CreateTestObject(),
                    CreateTestObject()
                }
            },
            "[\r\n" +
            "\t[\r\n" +
            "\t\tTestObject\r\n" +
            "\t\t\tInt1 = 10\r\n" +
            "\t\t\tStr = abc\r\n" +
            "\t\t\tInt2 = 12\r\n" +
            "\t\tTestObject\r\n" +
            "\t\t\tInt1 = 10\r\n" +
            "\t\t\tStr = abc\r\n" +
            "\t\t\tInt2 = 12\r\n" +
            "\t]\r\n" +
            "]"
        ).SetName("Nested multiline array"),
        new TestCaseData(
            new Dictionary<TestObject, TestObject>
            {
                {
                    CreateTestObject(),
                    CreateTestObject()
                }
            },
            "[\r\n" +
            "\t{\r\n" +
            "\t\tTestObject\r\n" +
            "\t\t\tInt1 = 10\r\n" +
            "\t\t\tStr = abc\r\n" +
            "\t\t\tInt2 = 12:\r\n" +
            "\t\tTestObject\r\n" +
            "\t\t\tInt1 = 10\r\n" +
            "\t\t\tStr = abc\r\n" +
            "\t\t\tInt2 = 12\r\n" +
            "\t}\r\n" +
            "]"
        ).SetName("Multiline dictionary")
    };

    public static readonly TestCaseData[] LoopReferences =
    {
        new TestCaseData(
            CreateCyclicObject,
            "CyclicTestObject\r\n" +
            "\tNumber = 1\r\n" +
            "\tChild1 = [Loop reference]\r\n" +
            "\tChild2 = CyclicTestObject\r\n" +
            "\t\tNumber = 2\r\n" +
            "\t\tChild1 = [Loop reference]\r\n" +
            "\t\tChild2 = [Loop reference]\r\n" +
            "\t\tChild3 = CyclicTestObject\r\n" +
            "\t\t\tNumber = 3\r\n" +
            "\t\t\tChild1 = [Loop reference]\r\n" +
            "\t\t\tChild2 = [Loop reference]\r\n" +
            "\t\t\tChild3 = [Loop reference]\r\n" +
            "\tChild3 = CyclicTestObject\r\n" +
            "\t\tNumber = 3\r\n" +
            "\t\tChild1 = [Loop reference]\r\n" +
            "\t\tChild2 = CyclicTestObject\r\n" +
            "\t\t\tNumber = 2\r\n" +
            "\t\t\tChild1 = [Loop reference]\r\n" +
            "\t\t\tChild2 = [Loop reference]\r\n" +
            "\t\t\tChild3 = [Loop reference]\r\n" +
            "\t\tChild3 = [Loop reference]"
        ).SetName("Int array")
    };

    private static TestObject CreateTestObject() =>
        new()
        {
            Int1 = 10,
            Int2 = 12,
            Str = "abc"
        };

    private static CyclicTestObject CreateCyclicObject()
    {
        var cyclic1 = new CyclicTestObject {Number = 1};
        var cyclic2 = new CyclicTestObject {Number = 2};
        var cyclic3 = new CyclicTestObject {Number = 3};
        cyclic1.Child1 = cyclic1;
        cyclic1.Child2 = cyclic2;
        cyclic1.Child3 = cyclic3;

        cyclic2.Child1 = cyclic1;
        cyclic2.Child2 = cyclic2;
        cyclic2.Child3 = cyclic3;

        cyclic3.Child1 = cyclic1;
        cyclic3.Child2 = cyclic2;
        cyclic3.Child3 = cyclic3;

        return cyclic1;
    }
}