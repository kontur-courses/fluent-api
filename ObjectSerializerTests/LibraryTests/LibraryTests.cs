using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using ObjectSerializer;
using ObjectSerializerTests.ClassesToSerialize;

namespace ObjectSerializerTests.LibraryTests
{
    [TestFixture]
    public class LibraryTests
    {
        private Library serializeLibrary;

        [OneTimeSetUp]
        public void Init()
        {
            serializeLibrary = new Library("Имени кого-то там");

            serializeLibrary.AddBook(new Book("1984", "George Orwell"));
            serializeLibrary.AddBook(new Book("To Kill a Mockingbird", "Harper Lee"));
            serializeLibrary.AddBook(new Book("The Great Gatsby", "F. Scott Fitzgerald"));
            serializeLibrary.AddBook(new Book("Pride and Prejudice", "Jane Austen"));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanPrint_CollectionObjects()
        {
            var printer = ObjectPrinter.For<Library>()
                .Exclude(l => l.BooksByAuthor);

            Approvals.Verify(printer.PrintToString(serializeLibrary));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanPrint_DictionaryObjects()
        {
            var printer = ObjectPrinter.For<Library>()
                .Exclude(l => l.Books);

            Approvals.Verify(printer.PrintToString(serializeLibrary));
        }

        [UseReporter(typeof(DiffReporter))]
        [Test]
        public void ObjectPrinter_CanPrint_DictionaryAndCollectionCorrectly()
        {
            var printer = ObjectPrinter.For<Library>()
                .For(l => l.Name)
                .Using(n => $"Отличная библиотека {n}");

            Approvals.Verify(printer.PrintToString(serializeLibrary));
        }
    }
}
