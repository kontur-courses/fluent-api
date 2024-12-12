using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using ObjectSerializer;
using ObjectSerializerTests.ClassesToSerialize;
using ObjectSerializerTests.ClassesToSerialize.Libraries;

namespace ObjectSerializerTests.LibraryTests
{
    [TestFixture]
    public class LibraryTests
    {
        private BaseLibrary baseLibrary;

        [OneTimeSetUp]
        public void Init()
        {
            baseLibrary = new BaseLibrary("Имени кого-то там");

            baseLibrary.AddBook(new Book("1984", "George Orwell"));
            baseLibrary.AddBook(new Book("To Kill a Mockingbird", "Harper Lee"));
            baseLibrary.AddBook(new Book("The Great Gatsby", "F. Scott Fitzgerald"));
            baseLibrary.AddBook(new Book("Pride and Prejudice", "Jane Austen"));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanPrint_CollectionObjects()
        {
            var printer = ObjectPrinter.For<BaseLibrary>()
                .Exclude(l => l.BooksByAuthor);

            Approvals.Verify(printer.PrintToString(baseLibrary));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanPrint_DictionaryObjects()
        {
            var printer = ObjectPrinter.For<BaseLibrary>()
                .Exclude(l => l.Books);

            Approvals.Verify(printer.PrintToString(baseLibrary));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanPrint_DictionaryAndCollectionCorrectly()
        {
            var printer = ObjectPrinter.For<BaseLibrary>()
                .Print(l => l.Name)
                .Using(n => $"Отличная библиотека {n}");

            Approvals.Verify(printer.PrintToString(baseLibrary));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanPrint_DictionaryOfDictionariesCorrectly()
        {
            var improvedLibrary = new ImprovedLibrary("Тут зачем-то считали слова");
            improvedLibrary.Books = baseLibrary.Books;
            improvedLibrary.AddWordOccurenceByAuthor("George Orwell", "beme", 10_000);
            improvedLibrary.AddWordOccurenceByAuthor("George Orwell", "Z", 3_000);
            improvedLibrary.AddWordOccurenceByAuthor("George Orwell", "Slon", 2_512);
            improvedLibrary.AddWordOccurenceByAuthor("Harper Lee", "pukmuk", 2_000);
            improvedLibrary.AddWordOccurenceByAuthor("Harper Lee", "52", 1_000_000);

            var printer = ObjectPrinter.For<ImprovedLibrary>()
                .Exclude(b => b.Books)
                .Exclude(b => b.BooksByAuthor);

            Approvals.Verify(printer.PrintToString(improvedLibrary));
        }
    }
}
