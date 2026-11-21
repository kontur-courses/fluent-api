using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectSerializerTests
{
    [TestCaseSource(nameof(FinalTypeValues))]
    public void PrintToString_ShouldPrintValueItself_WhenTypeIsFinal_Test(object value, string expectedPrinted)
    {
        var printer = ObjectPrinter.For<object>();

        var printed = printer.PrintToString(value);

        printed
            .Should()
            .Be(expectedPrinted);
    }

    [Test]
    public void PrintToString_ShouldPrintNull_WhenMemberValuesAreNull_Test()
    {
        var printer = ObjectPrinter.For<Node>();
        var expected = string.Join(
            Environment.NewLine,
            "Node",
            "\tName = null",
            "\tNext = null");

        var printed = printer.PrintToString(new Node());

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintNestedObjects_WhenMembersAreReferenceTypes_Test()
    {
        var node = new Node
        {
            Name = "first",
            Next = new Node
            {
                Name = "second",
                Next = new Node { Name = "third" }
            }
        };
        var printer = ObjectPrinter.For<Node>();
        var expected = string.Join(
            Environment.NewLine,
            "Node",
            "\tName = first",
            "\tNext = Node",
            "\t\tName = second",
            "\t\tNext = Node",
            "\t\t\tName = third",
            "\t\t\tNext = null");

        var printed = printer.PrintToString(node);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldSkipPropertiesOfExcludedType_WhenTheyAreInNestedObjects_Test()
    {
        var node = new Node { Name = "first", Next = new Node { Name = "second" } };
        var printer = ObjectPrinter
            .For<Node>()
            .ExcludeType<string>();
        var expected = string.Join(
            Environment.NewLine,
            "Node",
            "\tNext = Node",
            "\t\tNext = null");

        var printed = printer.PrintToString(node);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintCycleMark_WhenObjectReferencesItself_Test()
    {
        var node = new Node { Name = "Alex" };
        node.Next = node;
        var printer = ObjectPrinter.For<Node>();
        var expected = string.Join(
            Environment.NewLine,
            "Node",
            "\tName = Alex",
            "\tNext = <cycle>");

        var printed = printer.PrintToString(node);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintCycleMark_WhenObjectsReferenceEachOther_Test()
    {
        var first = new Node { Name = "first" };
        var second = new Node { Name = "second", Next = first };
        first.Next = second;
        var printer = ObjectPrinter.For<Node>();
        var expected = string.Join(
            Environment.NewLine,
            "Node",
            "\tName = first",
            "\tNext = Node",
            "\t\tName = second",
            "\t\tNext = <cycle>");

        var printed = printer.PrintToString(first);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintCycleMark_WhenCycleDoesNotIncludeRootObject_Test()
    {
        var third = new Node { Name = "third" };
        var second = new Node { Name = "second", Next = third };
        var first = new Node { Name = "first", Next = second };
        third.Next = second;
        var printer = ObjectPrinter.For<Node>();
        var expected = string.Join(
            Environment.NewLine,
            "Node",
            "\tName = first",
            "\tNext = Node",
            "\t\tName = second",
            "\t\tNext = Node",
            "\t\t\tName = third",
            "\t\t\tNext = <cycle>");

        var printed = printer.PrintToString(first);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintObjectEveryTime_WhenItIsReferencedTwiceWithoutCycle_Test()
    {
        var shared = new Node { Name = "shared" };
        var printer = ObjectPrinter.For<(Node, Node)>();
        var expected = string.Join(
            Environment.NewLine,
            "ValueTuple`2",
            "\tItem1 = Node",
            "\t\tName = shared",
            "\t\tNext = null",
            "\tItem2 = Node",
            "\t\tName = shared",
            "\t\tNext = null");

        var printed = printer.PrintToString((shared, shared));

        printed
            .Should()
            .Be(expected);
    }

    private static IEnumerable<TestCaseData> FinalTypeValues()
    {
        yield return new TestCaseData(true, "True");
        yield return new TestCaseData('a', "a");
        yield return new TestCaseData((byte)1, "1");
        yield return new TestCaseData((sbyte)-1, "-1");
        yield return new TestCaseData((short)-2, "-2");
        yield return new TestCaseData((ushort)2, "2");
        yield return new TestCaseData(3u, "3");
        yield return new TestCaseData(-4L, "-4");
        yield return new TestCaseData(4UL, "4");
        yield return new TestCaseData((nint)5, "5");
        yield return new TestCaseData((Int128)6, "6");
        yield return new TestCaseData(DayOfWeek.Monday, "Monday");
        yield return new TestCaseData(new TimeSpan(1, 2, 3), "01:02:03");
        yield return new TestCaseData(Guid.Parse("11111111-1111-1111-1111-111111111111"), "11111111-1111-1111-1111-111111111111");
    }
}