using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework.Internal;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class ObjectPrinterCollectionsTests
    {
        [Test]
        public void ObjectPrinter_PrintingList()
        {
            var testingList = new List<int> {1,2,3,100 };

            var outString = testingList.PrintToString();

            outString.Should().Be("List`1\r\n	1\r\n	2\r\n	3\r\n	100\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingHashSet()
        {
            var testingSet = new HashSet<int> { 1, 2, 3, 100 };

            var outString = testingSet.PrintToString();

            outString.Should().Be("HashSet`1\r\n	1\r\n	2\r\n	3\r\n	100\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingDictionary()
        {
            var testingDict = new Dictionary<string, string> {{ "a", "b"}, { "c", "d" } };

            var outString = testingDict.PrintToString();

            outString.Should().Be("Dictionary`2\r\n	KeyValuePair`2\r\n		Key = a\r\n		Value = b\r\n	KeyValuePair`2\r\n		Key = c\r\n		Value = d\r\n");
        }
    }
}
