using ApprovalTests.Reporters;
using ApprovalTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectSerializer;
using ObjectSerializerTests.ClassesToSerialize;

namespace ObjectSerializerTests.NestedCollectionTests
{
    [TestFixture]
    public class NestedCollectionTests
    {
        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanPrint_ListOfDictionaries()
        {
            var listDicts = new List<Dictionary<string, int>>()
            {
                new Dictionary<string, int>()
                {
                    {"bu", 1},
                    {"nu", 1},
                    {"mu", 1},
                },
                new Dictionary<string, int>()
                {
                    {"bu", 1},
                    {"nu", 1},
                    {"mu", 1},
                },
                new Dictionary<string, int>()
                {
                    {"bu", 1},
                    {"nu", 1},
                    {"mu", 1},
                },
            };

            var printer = ObjectPrinter.For<List<Dictionary<string, int>>>();

            Approvals.Verify(printer.PrintToString(listDicts));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanPrint_ListOfLists()
        {
            var listDicts = new List<List<Book>>()
            {
                new List<Book>()
                {
                    new Book("1984", "George Orwell"),
                    new Book("To Kill a Mockingbird", "Harper Lee")
                },
                new List<Book>()
                {
                    new Book("The Great Gatsby", "F. Scott Fitzgerald"),
                    new Book("Pride and Prejudice", "Jane Austen")
                }
            };

            var printer = ObjectPrinter.For<List<List<Book>>>();

            Approvals.Verify(printer.PrintToString(listDicts));
        }
    }
}
