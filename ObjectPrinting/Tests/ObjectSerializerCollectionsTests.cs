using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectSerializerCollectionsTests
{
    [Test]
    public void PrintToString_ShouldPrintEachElement_WhenObjectIsArray_Test()
    {
        var printer = ObjectPrinter.For<int[]>();
        var expected = string.Join(
            Environment.NewLine,
            "Int32[]",
            "\t1",
            "\t2",
            "\t3");

        var printed = printer.PrintToString([1, 2, 3]);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintEachElement_WhenObjectIsList_Test()
    {
        var printer = ObjectPrinter.For<List<string>>();
        var expected = string.Join(
            Environment.NewLine,
            "List`1",
            "\tAlex",
            "\tBob");

        var printed = printer.PrintToString(["Alex", "Bob"]);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintKeyValuePairs_WhenObjectIsDictionary_Test()
    {
        var dictionary = new Dictionary<int, string> { [1] = "Alex", [2] = "Bob" };
        var printer = ObjectPrinter.For<Dictionary<int, string>>();
        var expected = string.Join(
            Environment.NewLine,
            "Dictionary`2",
            "\t1 = Alex",
            "\t2 = Bob");

        var printed = printer.PrintToString(dictionary);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintOnlyTypeName_WhenCollectionIsEmpty_Test()
    {
        var printer = ObjectPrinter.For<List<int>>();

        var printed = printer.PrintToString([]);

        printed
            .Should()
            .Be("List`1");
    }

    [Test]
    public void PrintToString_ShouldPrintNull_WhenCollectionContainsNull_Test()
    {
        var printer = ObjectPrinter.For<List<string>>();
        var expected = string.Join(
            Environment.NewLine,
            "List`1",
            "\tAlex",
            "\tnull");

        var printed = printer.PrintToString(["Alex", null]);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintNestedCollections_WhenElementsAreCollections_Test()
    {
        var printer = ObjectPrinter.For<int[][]>();
        var expected = string.Join(
            Environment.NewLine,
            "Int32[][]",
            "\tInt32[]",
            "\t\t1",
            "\t\t2",
            "\tInt32[]",
            "\t\t3");

        var printed = printer.PrintToString([[1, 2], [3]]);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintMembersOfElements_WhenCollectionContainsObjects_Test()
    {
        var people = new List<Person>
        {
            new Person { Name = "Alex", Age = 19 },
            new Person { Name = "Bob", Age = 20 }
        };
        var printer = ObjectPrinter
            .For<List<Person>>()
            .Exclude<Guid>()
            .Exclude<double>();
        var expected = string.Join(
            Environment.NewLine,
            "List`1",
            "\tPerson",
            "\t\tName = Alex",
            "\t\tAge = 19",
            "\tPerson",
            "\t\tName = Bob",
            "\t\tAge = 20");

        var printed = printer.PrintToString(people);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintNestedValues_WhenDictionaryValuesAreCollections_Test()
    {
        var dictionary = new Dictionary<string, List<int>>
        {
            ["first"] = [1, 2],
            ["second"] = [3]
        };
        var printer = ObjectPrinter.For<Dictionary<string, List<int>>>();
        var expected = string.Join(
            Environment.NewLine,
            "Dictionary`2",
            "\tfirst = List`1",
            "\t\t1",
            "\t\t2",
            "\tsecond = List`1",
            "\t\t3");

        var printed = printer.PrintToString(dictionary);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintEachElement_WhenObjectIsAnyEnumerable_Test()
    {
        var queue = new Queue<int>([1, 2]);
        var printer = ObjectPrinter.For<Queue<int>>();
        var expected = string.Join(
            Environment.NewLine,
            "Queue`1",
            "\t1",
            "\t2");

        var printed = printer.PrintToString(queue);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldUseTypePrinterForElements_WhenItIsSetForElementType_Test()
    {
        var printer = ObjectPrinter
            .For<int[]>()
            .Printing<int>()
            .Using("X");
        var expected = string.Join(
            Environment.NewLine,
            "Int32[]",
            "\tA",
            "\tFF");

        var printed = printer.PrintToString([10, 255]);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintCollection_WhenItIsMemberOfObject_Test()
    {
        var tree = new TreeNode
        {
            Name = "root",
            Children =
            [
                new TreeNode { Name = "first" },
                new TreeNode { Name = "second" }
            ]
        };
        var printer = ObjectPrinter.For<TreeNode>();
        var expected = string.Join(
            Environment.NewLine,
            "TreeNode",
            "\tName = root",
            "\tChildren = List`1",
            "\t\tTreeNode",
            "\t\t\tName = first",
            "\t\t\tChildren = null",
            "\t\t\tParent = null",
            "\t\tTreeNode",
            "\t\t\tName = second",
            "\t\t\tChildren = null",
            "\t\t\tParent = null",
            "\tParent = null");

        var printed = printer.PrintToString(tree);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldUseMemberPrinter_WhenItIsSetForCollectionMember_Test()
    {
        var tree = new TreeNode
        {
            Name = "root",
            Children = [new TreeNode(), new TreeNode()]
        };
        var printer = ObjectPrinter
            .For<TreeNode>()
            .Printing(node => node.Children)
            .Using(children => children.Count + " children");
        var expected = string.Join(
            Environment.NewLine,
            "TreeNode",
            "\tName = root",
            "\tChildren = 2 children",
            "\tParent = null");

        var printed = printer.PrintToString(tree);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintCycleMark_WhenCollectionContainsItself_Test()
    {
        var list = new List<object>();
        list.Add(list);
        var printer = ObjectPrinter.For<List<object>>();
        var expected = string.Join(
            Environment.NewLine,
            "List`1",
            "\t<cycle>");

        var printed = printer.PrintToString(list);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintCycleMark_WhenDictionaryContainsItself_Test()
    {
        var dictionary = new Dictionary<string, object>();
        dictionary["self"] = dictionary;
        var printer = ObjectPrinter.For<Dictionary<string, object>>();
        var expected = string.Join(
            Environment.NewLine,
            "Dictionary`2",
            "\tself = <cycle>");

        var printed = printer.PrintToString(dictionary);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintCycleMark_WhenElementReferencesCollectionOwner_Test()
    {
        var root = new TreeNode { Name = "root" };
        var child = new TreeNode { Name = "child", Parent = root };
        root.Children = [child];
        var printer = ObjectPrinter.For<TreeNode>();
        var expected = string.Join(
            Environment.NewLine,
            "TreeNode",
            "\tName = root",
            "\tChildren = List`1",
            "\t\tTreeNode",
            "\t\t\tName = child",
            "\t\t\tChildren = null",
            "\t\t\tParent = <cycle>",
            "\tParent = null");

        var printed = printer.PrintToString(root);

        printed
            .Should()
            .Be(expected);
    }

    [Test]
    public void PrintToString_ShouldPrintCollectionEveryTime_WhenItIsReferencedTwiceWithoutCycle_Test()
    {
        var shared = new List<int> { 1, 2 };
        var printer = ObjectPrinter.For<(List<int>, List<int>)>();
        var expected = string.Join(
            Environment.NewLine,
            "ValueTuple`2",
            "\tItem1 = List`1",
            "\t\t1",
            "\t\t2",
            "\tItem2 = List`1",
            "\t\t1",
            "\t\t2");

        var printed = printer.PrintToString((shared, shared));

        printed
            .Should()
            .Be(expected);
    }
}
