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

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain(FormatCollectionEntry("0", "10")));
                Assert.That(result, Does.Contain(FormatCollectionEntry("1", "20")));
                Assert.That(result, Does.Contain(FormatCollectionEntry("2", "30")));
            });
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

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain(FormatCollectionEntry("0", "Alex")));
                Assert.That(result, Does.Contain(FormatCollectionEntry("1", "Bob")));
            });
        }

        [Test]
        public void PrintToString_PrintsDictionaryEntries()
        {
            var scores = new Dictionary<string, int>
            {
                ["Alex"] = 95,
                ["Bob"] = 87
            };

            var result = ObjectPrinter.For<Dictionary<string, int>>()
                .PrintToString(scores);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain(FormatCollectionEntry("Alex", "95")));
                Assert.That(result, Does.Contain(FormatCollectionEntry("Bob", "87")));
            });
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

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Members = "));
                Assert.That(result, Does.Contain(FormatCollectionEntry("0", "Alex")));
                Assert.That(result, Does.Contain(FormatCollectionEntry("1", "Bob")));
            });
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

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain(FormatCollectionEntry("0", "1")));
                Assert.That(result, Does.Contain(FormatCollectionEntry("1", "2")));
                Assert.That(result, Does.Contain(FormatCollectionEntry("0", "3")));
                Assert.That(result, Does.Contain(FormatCollectionEntry("1", "4")));
            });
        }

        [Test]
        public void PrintToString_HandlesEmptyCollections()
        {
            var result = ObjectPrinter.For<List<int>>()
                .PrintToString(new List<int>());

            Assert.That(result, Does.Contain(PrintingFormat.EmptyCollectionMarker));
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

            Assert.That(result, Does.Contain(FormatCollectionEntry("0", PrintingFormat.NullValue)));
            Assert.That(result, Does.Contain(FormatCollectionEntry("1", "Alex")));
        }

        [Test]
        public void PrintToString_HandlesCircularReferencesInsideLists()
        {
            var values = new List<object>();
            values.Add(values);

            var result = ObjectPrinter.For<List<object>>()
                .PrintToString(values);

            Assert.That(result, Does.Contain(FormatCollectionEntry("0", PrintingFormat.CircularReferenceMarker)));
        }

        [Test]
        public void PrintToString_HandlesCircularReferencesInsideDictionaries()
        {
            var values = new Dictionary<string, object>();
            values["self"] = values;

            var result = ObjectPrinter.For<Dictionary<string, object>>()
                .PrintToString(values);

            Assert.That(result, Does.Contain(FormatCollectionEntry("self", PrintingFormat.CircularReferenceMarker)));
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

            var occurrences = result.Split("Name = Shared").Length - 1;

            Assert.Multiple(() =>
            {
                Assert.That(occurrences, Is.EqualTo(2));
                Assert.That(result, Does.Not.Contain(PrintingFormat.CircularReferenceMarker));
            });
        }
        
        private static string FormatCollectionEntry(string key, string value)
        {
            return PrintingFormat.CollectionEntryPrefix +
                   key +
                   PrintingFormat.CollectionEntrySuffix +
                   PrintingFormat.MemberAssignmentSeparator +
                   value;
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