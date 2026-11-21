using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;
using static ObjectPrinting.Tests.TestOutput;

namespace ObjectPrinting.Tests;

[TestFixture]
public class PrintingConfigTests
{
    [TestCaseSource(nameof(ExclusionCases))]
    public void Exclude_OmitsConfiguredTypesAndMembers(Func<string> serialize, string expected)
    {
        var result = serialize();

        result.Should().Be(expected);
    }

    [TestCaseSource(nameof(CustomSerializerCases))]
    public void CustomSerializer_FormatsMatchingTypesAndMembers(Func<string> serialize, string expected)
    {
        var result = serialize();

        result.Should().Be(expected);
    }

    [TestCaseSource(nameof(CultureCases))]
    public void Culture_FormatsNumericTypesAndMembers(Func<string> serialize, string expected)
    {
        var result = serialize();

        result.Should().Be(expected);
    }

    [TestCaseSource(nameof(TruncationCases))]
    public void Truncate_LimitsStringLength(Func<string> serialize, string expected)
    {
        var result = serialize();

        result.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> ExclusionCases()
    {
        var sample = new MixedTypes { Text = "Alice", Count = 30, Flag = true };
        var pair = new TwoInts { First = 1, Second = 2 };
        var withField = new ObjectWithField { PublicProperty = "prop", PublicField = 99 };

        yield return new TestCaseData(
                () => sample.PrintToString(cfg => cfg.Exclude<int>()),
                Lines(
                    nameof(MixedTypes),
                    "\tText = Alice",
                    "\tFlag = True"))
            .SetName("Exclude_SingleType");

        yield return new TestCaseData(
                () => sample.PrintToString(cfg => cfg.Exclude<int>().Exclude<bool>()),
                Lines(
                    nameof(MixedTypes),
                    "\tText = Alice"))
            .SetName("Exclude_MultipleChainedTypes");

        yield return new TestCaseData(
                () => pair.PrintToString(cfg => cfg.Exclude(x => x.First)),
                Lines(
                    nameof(TwoInts),
                    "\tSecond = 2"))
            .SetName("Exclude_SpecificProperty");

        yield return new TestCaseData(
                () => withField.PrintToString(cfg => cfg.Exclude(x => x.PublicField)),
                Lines(
                    nameof(ObjectWithField),
                    "\tPublicProperty = prop"))
            .SetName("Exclude_PublicField");
    }

    private static IEnumerable<TestCaseData> CustomSerializerCases()
    {
        var pair = new TwoInts { First = 10, Second = 255 };
        var withField = new ObjectWithField { PublicProperty = "ok", PublicField = 7 };

        yield return new TestCaseData(
                () => pair.PrintToString(cfg => cfg.For<int>().Using(i => i.ToString("X"))),
                Lines(
                    nameof(TwoInts),
                    "\tFirst = A",
                    "\tSecond = FF"))
            .SetName("CustomSerializer_TypeLevel");

        yield return new TestCaseData(
                () => pair.PrintToString(cfg => cfg
                    .For<int>().Using(i => $"T:{i}")
                    .For(x => x.First).Using(i => $"M:{i}")),
                Lines(
                    nameof(TwoInts),
                    "\tFirst = M:10",
                    "\tSecond = T:255"))
            .SetName("CustomSerializer_MemberOverridesType");

        yield return new TestCaseData(
                () => withField.PrintToString(cfg => cfg.For(x => x.PublicField).Using(v => $"field={v}")),
                Lines(
                    nameof(ObjectWithField),
                    "\tPublicProperty = ok",
                    "\tPublicField = field=7"))
            .SetName("CustomSerializer_PublicField");
    }

    private static IEnumerable<TestCaseData> CultureCases()
    {
        var ru = CultureInfo.GetCultureInfo("ru-RU");
        var inv = CultureInfo.InvariantCulture;
        var numbers = new TwoDoubles { First = 1.5, Second = 2.75 };

        yield return new TestCaseData(
                () => numbers.PrintToString(cfg => cfg.For<double>().Using(ru)),
                Lines(
                    nameof(TwoDoubles),
                    "\tFirst = 1,5",
                    "\tSecond = 2,75"))
            .SetName("Culture_TypeLevel_RuRU");

        yield return new TestCaseData(
                () => numbers.PrintToString(cfg => cfg.For<double>().Using(inv)),
                Lines(
                    nameof(TwoDoubles),
                    "\tFirst = 1.5",
                    "\tSecond = 2.75"))
            .SetName("Culture_TypeLevel_Invariant");

        yield return new TestCaseData(
                () => numbers.PrintToString(cfg => cfg
                    .For<double>().Using(inv)
                    .For(x => x.First).Using(ru)),
                Lines(
                    nameof(TwoDoubles),
                    "\tFirst = 1,5",
                    "\tSecond = 2.75"))
            .SetName("Culture_MemberOverridesType");
    }

    private static IEnumerable<TestCaseData> TruncationCases()
    {
        var sample = new TwoStrings { First = "HelloWorld", Second = "HelloWorld" };

        yield return new TestCaseData(
                () => sample.PrintToString(cfg => cfg.For<string>().Truncate(3)),
                Lines(
                    nameof(TwoStrings),
                    "\tFirst = Hel",
                    "\tSecond = Hel"))
            .SetName("Truncate_TypeLevel_Max3");

        yield return new TestCaseData(
                () => sample.PrintToString(cfg => cfg
                    .For<string>().Truncate(3)
                    .For(x => x.First).Truncate(6)),
                Lines(
                    nameof(TwoStrings),
                    "\tFirst = HelloW",
                    "\tSecond = Hel"))
            .SetName("Truncate_MemberOverridesType_First6_Second3");

        yield return new TestCaseData(
                () => sample.PrintToString(cfg => cfg.For(x => x.First).Truncate(0)),
                Lines(
                    nameof(TwoStrings),
                    "\tFirst = ",
                    "\tSecond = HelloWorld"))
            .SetName("Truncate_Boundary_ZeroLength");

        yield return new TestCaseData(
                () => sample.PrintToString(cfg => cfg.For(x => x.First).Truncate(10)),
                Lines(
                    nameof(TwoStrings),
                    "\tFirst = HelloWorld",
                    "\tSecond = HelloWorld"))
            .SetName("Truncate_Boundary_ExactLength");

        yield return new TestCaseData(
                () => sample.PrintToString(cfg => cfg.For(x => x.First).Truncate(50)),
                Lines(
                    nameof(TwoStrings),
                    "\tFirst = HelloWorld",
                    "\tSecond = HelloWorld"))
            .SetName("Truncate_Boundary_ExceedingLength");

        yield return new TestCaseData(
                () => sample.PrintToString(cfg => cfg
                    .For(x => x.First).Using(s => s.ToUpperInvariant())
                    .For(x => x.First).Truncate(5)),
                Lines(
                    nameof(TwoStrings),
                    "\tFirst = HELLO",
                    "\tSecond = HelloWorld"))
            .SetName("Truncate_CombinedWithMemberSerializer");
    }

    private class MixedTypes
    {
        public string Text { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool Flag { get; set; }
    }

    private class TwoInts
    {
        public int First { get; set; }
        public int Second { get; set; }
    }

    private class TwoDoubles
    {
        public double First { get; set; }
        public double Second { get; set; }
    }

    private class TwoStrings
    {
        public string First { get; set; } = string.Empty;
        public string Second { get; set; } = string.Empty;
    }

    private class ObjectWithField
    {
        public int PublicField;
        public string PublicProperty { get; set; } = string.Empty;
    }
}
