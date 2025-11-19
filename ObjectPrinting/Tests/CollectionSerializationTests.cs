using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class CollectionSerializationTests : TestBase
{
    [Test]
    public void PrintToString_WithList_SerializesListElements()
    {
        var list = CreateTestPeopleList();
        var result = list.PrintToString();

        result.Should()
            .Contain("List`1<Person>")
            .And.Contain("[0] = Person")
            .And.Contain("[1] = Person")
            .And.Contain("Name = First")
            .And.Contain("Name = Second")
            .And.Contain("Count = 2");
    }

    [Test]
    public void PrintToString_WithArray_SerializesArrayElements()
    {
        var array = CreateTestStringArray();
        var result = array.PrintToString();

        result.Should()
            .Contain("String[]")
            .And.Contain("[0] = apple")
            .And.Contain("[1] = banana")
            .And.Contain("[2] = cherry");
    }

    [Test]
    public void PrintToString_WithDictionary_SerializesKeyValuePairs()
    {
        var dict = CreateTestDictionary();
        var result = dict.PrintToString();

        result.Should()
            .Contain("Dictionary`2<String, Int32>")
            .And.Contain("Key = first")
            .And.Contain("Value = 1")
            .And.Contain("Key = second")
            .And.Contain("Value = 2")
            .And.Contain("Key = third")
            .And.Contain("Value = 3")
            .And.Contain("Count = 3");
    }


    [Test]
    public void PrintToString_WithEmptyCollection_ShowsEmptyCollection()
    {
        var result = new List<string>().PrintToString();

        result.Should()
            .Contain("List`1<String>")
            .And.Contain("Count = 0");
    }

    [Test]
    public void PrintToString_WithNestedCollections_SerializesRecursively()
    {
        var matrix = new List<List<int>>
        {
            new List<int> { 1, 2, 3 },
            new List<int> { 4, 5, 6 }
        };

        var result = matrix.PrintToString();

        result.Should()
            .Contain("List`1<List`1>")
            .And.Contain("[0] = List`1<Int32>")
            .And.Contain("[1] = List`1<Int32>")
            .And.Contain("[0] = 1")
            .And.Contain("[1] = 2")
            .And.Contain("[2] = 3");
    }

    [Test]
    public void
        PrintToString_WithCollectionContainingNull_HandlesNullElements()
    {
        var listWithNull = new List<string> { "first", null, "third" };
        var result = listWithNull.PrintToString();

        result.Should()
            .Contain("[0] = first")
            .And.Contain("[1] = null")
            .And.Contain("[2] = third");
    }
}