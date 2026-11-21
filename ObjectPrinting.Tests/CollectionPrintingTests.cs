using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class CollectionPrintingTests
    {
        [Test]
        public void PrintToString_PrintsArrayElements()
        {
            var numbers = new[] { 10, 20, 30 };

            var result = ObjectPrinter.For<int[]>()
                .PrintToString(numbers);

            var expected =
                "\t[0] = 10" + PrintingTestFormat.NewLine +
                "\t[1] = 20" + PrintingTestFormat.NewLine +
                "\t[2] = 30" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_PrintsMultidimensionalArrayElements()
        {
            var numbers = new[,]
            {
                { 1, 2 },
                { 3, 4 }
            };

            var result = ObjectPrinter.For<int[,]>()
                .PrintToString(numbers);

            var expected =
                "\t[0,0] = 1" + PrintingTestFormat.NewLine +
                "\t[0,1] = 2" + PrintingTestFormat.NewLine +
                "\t[1,0] = 3" + PrintingTestFormat.NewLine +
                "\t[1,1] = 4" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_PreservesNonZeroArrayLowerBounds()
        {
            var numbers = Array.CreateInstance(
                typeof(int),
                new[] { 2, 2 },
                new[] { 1, -1 });

            numbers.SetValue(10, new[] { 1, -1 });
            numbers.SetValue(20, new[] { 1, 0 });
            numbers.SetValue(30, new[] { 2, -1 });
            numbers.SetValue(40, new[] { 2, 0 });

            var result = ObjectPrinter.For<Array>()
                .PrintToString(numbers);

            var expected =
                "\t[1,-1] = 10" + PrintingTestFormat.NewLine +
                "\t[1,0] = 20" + PrintingTestFormat.NewLine +
                "\t[2,-1] = 30" + PrintingTestFormat.NewLine +
                "\t[2,0] = 40" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_PrintsListElements()
        {
            var members = new List<string>
            {
                "Alex",
                "Bob"
            };

            var result = ObjectPrinter.For<List<string>>()
                .PrintToString(members);

            var expected =
                "\t[0] = Alex" + PrintingTestFormat.NewLine +
                "\t[1] = Bob" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_PrintsSortedDictionaryEntries()
        {
            var scores = new SortedDictionary<string, int>
            {
                ["Alex"] = 95,
                ["Bob"] = 87
            };

            var result = ObjectPrinter.For<SortedDictionary<string, int>>()
                .PrintToString(scores);

            var expected =
                "\t[Alex] = 95" + PrintingTestFormat.NewLine +
                "\t[Bob] = 87" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_PrintsCollectionsInsideObjects()
        {
            var team = new Team
            {
                Members = new List<string>
                {
                    "Alex",
                    "Bob"
                }
            };

            var result = ObjectPrinter.For<Team>()
                .PrintToString(team);

            var expected =
                "Team" + PrintingTestFormat.NewLine +
                "\tMembers = \t\t[0] = Alex" + PrintingTestFormat.NewLine +
                "\t\t[1] = Bob" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_PrintsNestedCollections()
        {
            var groups = new List<List<int>>
            {
                new() { 1, 2 },
                new() { 3, 4 }
            };

            var result = ObjectPrinter.For<List<List<int>>>()
                .PrintToString(groups);

            var expected =
                "\t[0] = \t\t[0] = 1" + PrintingTestFormat.NewLine +
                "\t\t[1] = 2" + PrintingTestFormat.NewLine +
                "\t[1] = \t\t[0] = 3" + PrintingTestFormat.NewLine +
                "\t\t[1] = 4" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_HandlesEmptyCollections()
        {
            var result = ObjectPrinter.For<List<int>>()
                .PrintToString(new List<int>());

            var expected =
                PrintingTestFormat.IndentationCharacter +
                PrintingTestFormat.EmptyCollectionMarker +
                PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_HandlesNullCollectionElements()
        {
            var members = new List<string?>
            {
                null,
                "Alex"
            };

            var result = ObjectPrinter.For<List<string?>>()
                .PrintToString(members);

            var expected =
                "\t[0] = null" + PrintingTestFormat.NewLine +
                "\t[1] = Alex" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_HandlesCircularReferencesInsideLists()
        {
            var values = new List<object>();
            values.Add(values);

            var result = ObjectPrinter.For<List<object>>()
                .PrintToString(values);

            var expected =
                "\t[0] = [Circular reference]" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_HandlesCircularReferencesInsideDictionaries()
        {
            var values = new Dictionary<string, object>();
            values["self"] = values;

            var result = ObjectPrinter.For<Dictionary<string, object>>()
                .PrintToString(values);

            var expected =
                "\t[self] = [Circular reference]" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_SerializesSharedObjectsInCollection()
        {
            var shared = new PersonNode
            {
                Name = "Shared"
            };

            var members = new List<PersonNode>
            {
                shared,
                shared
            };

            var result = ObjectPrinter.For<List<PersonNode>>()
                .PrintToString(members);

            var expected =
                "\t[0] = PersonNode" + PrintingTestFormat.NewLine +
                "\t\tName = Shared" + PrintingTestFormat.NewLine +
                "\t[1] = PersonNode" + PrintingTestFormat.NewLine +
                "\t\tName = Shared" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintingType_UsesFormatterForDictionaryKeys()
        {
            var values = new Dictionary<int, string>
            {
                [7] = "Seven"
            };

            var result = ObjectPrinter.For<Dictionary<int, string>>()
                .Printing<int>()
                .Using(number => $"key-{number}")
                .PrintToString(values);

            var expected =
                "\t[key-7] = Seven" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_HandlesEmptyArrays()
        {
            var result = ObjectPrinter.For<int[]>()
                .PrintToString(Array.Empty<int>());

            var expected =
                PrintingTestFormat.IndentationCharacter +
                PrintingTestFormat.EmptyCollectionMarker +
                PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_HandlesEmptyDictionaries()
        {
            var result = ObjectPrinter.For<Dictionary<string, int>>()
                .PrintToString(new Dictionary<string, int>());

            var expected =
                PrintingTestFormat.IndentationCharacter +
                PrintingTestFormat.EmptyCollectionMarker +
                PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }


        private sealed class Team
        {
            public List<string> Members { get; set; } = [];
        }

        private sealed class PersonNode
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}