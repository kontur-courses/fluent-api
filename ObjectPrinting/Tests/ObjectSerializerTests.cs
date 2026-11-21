using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectSerializerTests
{
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
            .Exclude<string>();
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
}