using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using static ObjectPrinting.Tests.TestOutput;

namespace ObjectPrinting.Tests;

[TestFixture]
public class CollectionSerializationTests
{
    [TestCaseSource(nameof(BasicCollectionCases))]
    public void CollectionsAndDictionaries_SerializeWithExpectedFormat(object collection, string expected)
    {
        var result = collection.PrintToString();

        result.Should().Be(expected);
    }

    [TestCaseSource(nameof(CyclicCollectionCases))]
    public void CyclicCollections_TerminateWithCyclicMarker(Func<string> serialize, string expected)
    {
        var result = serialize();

        result.Should().Be(expected);
    }

    [TestCaseSource(nameof(CollectionConfigRuleCases))]
    public void ConfigRules_ApplyToCollectionsAndDictionaries(Func<string> serialize, string expected)
    {
        var result = serialize();

        result.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> BasicCollectionCases()
    {
        yield return new TestCaseData(
                new[] { 10, 20, 30 },
                Lines(
                    "Int32[]",
                    "\t[0] = 10",
                    "\t[1] = 20",
                    "\t[2] = 30"))
            .SetName("Collection_Array");

        yield return new TestCaseData(
                new HashSet<int> { 7 },
                Lines(
                    "HashSet`1",
                    "\t[0] = 7"))
            .SetName("Collection_NonIndexedEnumerable_HashSet");

        yield return new TestCaseData(
                new List<int>(),
                Lines("List`1"))
            .SetName("Collection_EmptyList");

        yield return new TestCaseData(
                new[] { "a", null, "b" },
                Lines(
                    "String[]",
                    "\t[0] = a",
                    "\t[1] = null",
                    "\t[2] = b"))
            .SetName("Collection_WithNullElement");

        yield return new TestCaseData(
                new List<int[]> { new[] { 1, 2 }, new[] { 3 } },
                Lines(
                    "List`1",
                    "\t[0] = Int32[]",
                    "\t\t[0] = 1",
                    "\t\t[1] = 2",
                    "\t[1] = Int32[]",
                    "\t\t[0] = 3"))
            .SetName("Collection_NestedCollections");

        yield return new TestCaseData(
                new List<Item> { new("Alice"), new("Bob") },
                Lines(
                    "List`1",
                    "\t[0] = Item",
                    "\t\tName = Alice",
                    "\t[1] = Item",
                    "\t\tName = Bob"))
            .SetName("Collection_OfComplexObjects");

        yield return new TestCaseData(
                new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 },
                Lines(
                    "Dictionary`2",
                    "\t[one] = 1",
                    "\t[two] = 2"))
            .SetName("Dictionary_KeyValuePairs");

        yield return new TestCaseData(
                new Dictionary<string, int>(),
                Lines("Dictionary`2"))
            .SetName("Dictionary_Empty");

        yield return new TestCaseData(
                new Dictionary<string, string?> { ["k"] = null },
                Lines(
                    "Dictionary`2",
                    "\t[k] = null"))
            .SetName("Dictionary_WithNullValue");
    }

    private static IEnumerable<TestCaseData> CyclicCollectionCases()
    {
        yield return new TestCaseData(
                () =>
                {
                    var list = new List<object>();
                    list.Add(list);
                    return list.PrintToString();
                },
                Lines(
                    "List`1",
                    "\t[0] = <cyclic reference>"))
            .SetName("CyclicCollection_ListSelfReference");

        yield return new TestCaseData(
                () =>
                {
                    var dict = new Dictionary<string, object>();
                    dict["self"] = dict;
                    return dict.PrintToString();
                },
                Lines(
                    "Dictionary`2",
                    "\t[self] = <cyclic reference>"))
            .SetName("CyclicCollection_DictionarySelfReference");
    }

    private static IEnumerable<TestCaseData> CollectionConfigRuleCases()
    {
        yield return new TestCaseData(
                () => new List<string> { "HelloWorld", "Test" }
                    .PrintToString(cfg => cfg.For<string>().Truncate(5)),
                Lines(
                    "List`1",
                    "\t[0] = Hello",
                    "\t[1] = Test"))
            .SetName("CollectionConfig_TypeRuleAppliesToElements");

        yield return new TestCaseData(
                () => new List<object> { 1, "keep", 2 }
                    .PrintToString(cfg => cfg.Exclude<int>()),
                Lines(
                    "List`1",
                    "\t[1] = keep"))
            .SetName("CollectionConfig_ExcludeElementTypeInList");

        yield return new TestCaseData(
                () => new Dictionary<string, object> { ["a"] = 1, ["b"] = "keep" }
                    .PrintToString(cfg => cfg.Exclude<int>()),
                Lines(
                    "Dictionary`2",
                    "\t[b] = keep"))
            .SetName("CollectionConfig_ExcludeValueTypeInDictionary");

        yield return new TestCaseData(
                () => new Dictionary<object, string> { [1] = "drop", ["k"] = "keep" }
                    .PrintToString(cfg => cfg.Exclude<int>()),
                Lines(
                    "Dictionary`2",
                    "\t[k] = keep"))
            .SetName("CollectionConfig_ExcludeKeyTypeInDictionary");

        yield return new TestCaseData(
                () => new List<int> { 1, 2, 3 }
                    .PrintToString(cfg => cfg.For<List<int>>().Using(list => $"[{string.Join(", ", list)}]")),
                Lines("[1, 2, 3]"))
            .SetName("CollectionConfig_CustomSerializerOverridesDefault");
    }

    private class Item(string name)
    {
        public string Name { get; } = name;
    }
}
