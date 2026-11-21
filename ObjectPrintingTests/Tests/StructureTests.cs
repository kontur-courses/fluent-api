using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Solved;

namespace ObjectPrintingTests.Tests
{
    [TestFixture]
    public class StructureTests
    {
        [Test]
        public void PrintToString_CyclicReference_CompletesWithCycleMarker()
        {
            var first = new Node { Name = "a" };
            var second = new Node { Name = "b", Next = first };
            first.Next = second;

            var result = first.PrintToString();

            result.Should().Contain("<cycle>");
        }

        [Test]
        public void PrintToString_ListWithRepeatedReferences_PrintsEachOccurrence()
        {
            var shared = new Node { Name = "shared" };
            var list = new List<Node> { shared, shared, shared, shared, shared };

            var result = list.PrintToString();

            result.Should().NotContain("<cycle>");
            var occurrences = result.Split("Name = shared").Length - 1;
            occurrences.Should().Be(5);
        }

        [Test]
        public void PrintToString_ListWithRepeatedCyclicReferences_PrintsCycleInsideEach()
        {
            var shared = new Node { Name = "shared" };
            shared.Next = shared;
            var list = new List<Node> { shared, shared };

            var result = list.PrintToString();

            var occurrences = result.Split("Name = shared").Length - 1;
            occurrences.Should().Be(2);
            result.Split("<cycle>").Length.Should().Be(3);
        }

        [Test]
        public void PrintToString_DeepChainWithMaxDepth_CollapsesDeeperLevels()
        {
            var me = new TreeNode { Name = "Я" };
            var father = new TreeNode { Name = "Отец", Father = me };
            var grandfather = new TreeNode { Name = "Дед", Father = father };
            var greatGrandfather = new TreeNode { Name = "Прадед", Father = grandfather };

            var result = ObjectPrinter
                .For<TreeNode>()
                .PrintToString(greatGrandfather, maxDepth: 3);

            result.Should().Contain("Прадед")
                  .And.Contain("Дед")
                  .And.Contain("Отец")
                  .And.Contain("<TreeNode>")
                  .And.NotContain("Я");
        }

        [Test]
        public void PrintToString_MaxDepthZero_CollapsesRoot()
        {
            var person = new Person { Name = "Alex" };

            var result = ObjectPrinter
                .For<Person>()
                .PrintToString(person, maxDepth: 0);

            result.Should().Contain("<Person>").And.NotContain("Alex");
        }

        [Test]
        public void PrintToString_List_PrintsExactSequence()
        {
            var list = new List<int> { 10, 20, 30 };

            var result = list.PrintToString().Trim();

            result.Should().Be("[10, 20, 30]");
        }

        [Test]
        public void PrintToString_EmptyList_PrintsEmptyBrackets()
        {
            var list = new List<int>();

            var result = list.PrintToString().Trim();

            result.Should().Be("[]");
        }

        [Test]
        public void PrintToString_Array_PrintsExactSequence()
        {
            var array = new[] { 10, 20, 30 };

            var result = array.PrintToString().Trim();

            result.Should().Be("[10, 20, 30]");
        }

        [Test]
        public void PrintToString_EmptyArray_PrintsEmptyBrackets()
        {
            var array = Array.Empty<int>();

            var result = array.PrintToString().Trim();

            result.Should().Be("[]");
        }

        [Test]
        public void PrintToString_StringArray_PrintsExactSequence()
        {
            var array = new[] { "a", "b" };

            var result = array.PrintToString().Trim();

            result.Should().Be("[a, b]");
        }

        [Test]
        public void PrintToString_StringList_PrintsExactSequence()
        {
            var list = new List<string> { "a", "b" };

            var result = list.PrintToString().Trim();

            result.Should().Be("[a, b]");
        }

        [Test]
        public void PrintToString_ComplexObjectArray_PrintsEachElementExpanded()
        {
            var array = new[]
            {
                new Node { Name = "first" },
                new Node { Name = "second" },
            };

            var result = array.PrintToString();

            result.Should().Contain("Name = first");
            result.Should().Contain("Name = second");
            result.Should().NotContain("<cycle>");
        }

        [Test]
        public void PrintToString_NestedLists_PrintsExactNestedBrackets()
        {
            var nested = new List<List<int>> { new() { 1, 2 }, new() { 3, 4 } };

            var result = nested.PrintToString().Trim();

            result.Should().Be("[[1, 2], [3, 4]]");
        }

        [Test]
        public void PrintToString_Dictionary_PrintsExactKeyValuePairs()
        {
            var dictionary = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

            var result = dictionary.PrintToString();

            var lines = result
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .ToArray();

            lines.Should().BeEquivalentTo(new[] { "\t[a] = 1", "\t[b] = 2" });
        }
    }
}