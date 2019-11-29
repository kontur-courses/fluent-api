using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterCollectionsTests
    {
        private static readonly object[] SourceIEnumerables =
        {
            new object[]
            {
                new Tuple<IEnumerable, string>(
                    new Dictionary<string, string> {{"a", "b"}, {"c", "d"}}
                    , "Dictionary`2\r\n	KeyValuePair`2\r\n		Key = a\r\n		Value = b\r\n	KeyValuePair`2\r\n		Key = c\r\n		Value = d\r\n")
            },
            new object[]
            {
                new Tuple<IEnumerable, string>(
                    new List<int> {1, 2}
                    , "List`1\r\n	1\r\n	2\r\n"
                )
            }
        };

        [Test]
        public void ObjectPrinter_PrintingConcurrentDictionary()
        {
            var testingDict = new ConcurrentDictionary<string, string>();
            testingDict.AddOrUpdate("a", "b", (key, oldValue) => "b");
            testingDict.AddOrUpdate("c", "d", (key, oldValue) => "d");

            var outString = testingDict.PrintToString();

            outString.Should()
                .Be(
                    "ConcurrentDictionary`2\r\n	KeyValuePair`2\r\n		Key = c\r\n		Value = d\r\n	KeyValuePair`2\r\n		Key = a\r\n		Value = b\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingDictionary()
        {
            var testingDict = new Dictionary<string, string> {{"a", "b"}, {"c", "d"}};

            var outString = testingDict.PrintToString();

            outString.Should()
                .Be(
                    "Dictionary`2\r\n	KeyValuePair`2\r\n		Key = a\r\n		Value = b\r\n	KeyValuePair`2\r\n		Key = c\r\n		Value = d\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingHashSet()
        {
            var testingSet = new HashSet<int> {1, 2, 3, 100};

            var outString = testingSet.PrintToString();

            outString.Should().Be("HashSet`1\r\n	1\r\n	2\r\n	3\r\n	100\r\n");
        }

        [Test]
        [TestCaseSource(nameof(SourceIEnumerables))]
        public void ObjectPrinter_PrintingIEnumerable(Tuple<IEnumerable, string> inputParameters)
        {
            var outString = inputParameters.Item1.PrintToString();

            outString.Should().Be(inputParameters.Item2);
        }

        [Test]
        public void ObjectPrinter_PrintingList()
        {
            var testingList = new List<int> {1, 2, 3, 100};

            var outString = testingList.PrintToString();

            outString.Should().Be("List`1\r\n	1\r\n	2\r\n	3\r\n	100\r\n");
        }
    }
}