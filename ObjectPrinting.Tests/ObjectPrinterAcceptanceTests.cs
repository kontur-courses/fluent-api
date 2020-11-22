using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", BirthDate = new DateTime(2001, 2, 3)};
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingGuid()
        {
            printer.Excluding<Guid>();

            printer.PrintToString(person).Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    null,
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n"));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingGuid()
        {
            printer.Printing<Guid>().Using(i => "Guid");

            printer.PrintToString(person).Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    "Guid",
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n"));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingDateTimeWithCulture()
        {
            printer.Printing<DateTime>().Using(CultureInfo.InvariantCulture);

            printer.PrintToString(person).Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person), 
                    1,
                    "Guid\r\n", 
                    "Alex\r\n",
                    "02/03/2001 00:00:00\r\n"));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingParentAsMember()
        {
            printer.Printing(p => p.Parent).Using(p => "!Parent!");

            printer.PrintToString(person).Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    "Guid\r\n",
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n",
                    "!Parent!"));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingWithCutting()
        {
            printer.Printing(p => p.Name).TrimmedToLength(3);

            printer.PrintToString(person).Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    "Guid\r\n",
                    "Ale",
                    "03.02.2001 0:00:00\r\n"));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingId()
        {
            printer.Excluding(p => p.Id);

            printer.PrintToString(person).Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1, 
                    null, 
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n"));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenCycleBetweenObjectsDifferentTypes()
        {
            person.Parent = new Parent
            {
                Name = "John",
                Child = person,
                BirthDate = new DateTime(1975, 2, 3)
            };

            printer.PrintToString(person).Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1, 
                    "Guid\r\n", 
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n",
                    GetCorrectPrintingConfig(
                        nameof(Parent),
                        2,
                        "Guid\r\n", 
                        "John\r\n",
                        "03.02.1975 0:00:00\r\n",
                        "null\r\n", 
                        "cycle\r\n")));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenCycleBetweenObjectsSameTypes()
        {
            var parent = new Parent {BirthDate = new DateTime(1975, 2, 3)};
            parent.Parent = parent;

            printer.PrintToString(parent).Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Parent),
                    1,
                    "Guid\r\n",
                    "null\r\n",
                    "03.02.1975 0:00:00\r\n",
                    "cycle\r\n", 
                    "null\r\n"));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingArray()
        {
            var library = new Library();
            var printerLibrary = new PrintingConfig<Library>().Excluding<List<Book>>()
                .Excluding<Dictionary<string, Book>>();

            printerLibrary.PrintToString(library).Should().Be(
                GetCorrectPrintingArray(
                    nameof(Library.BooksArray),
                    library.BooksArray.GetType().Name));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingList()
        {
            var library = new Library();
            var printerLibrary =
                new PrintingConfig<Library>().Excluding<Book[]>().Excluding<Dictionary<string, Book>>();

            printerLibrary.PrintToString(library).Should().Be(
                GetCorrectPrintingArray(
                    nameof(Library.BooksList),
                    library.BooksList.GetType().Name));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingDictionary()
        {
            var library = new Library();
            var printerLibrary = new PrintingConfig<Library>().Excluding<Book[]>().Excluding<List<Book>>();

            var dict = library.BooksDictionary.First();
            var result = $"{nameof(Library)}\r\n\t{nameof(Library.BooksDictionary)} = " +
                         $"{library.BooksDictionary.GetType().Name}\r\n\t\t" +
                         $"{dict.GetType().Name}\r\n\t\t\t" +
                         $"{nameof(dict.Key)} = Alex\r\n\t\t\t" +
                         $"{nameof(dict.Value)} = {nameof(Book)}\r\n\t\t\t\t" +
                         $"{nameof(Book.Name)} = MyBook\r\n\t\t\t\t{nameof(Book.Author)} = Alex\r\n";

            printerLibrary.PrintToString(library).Should().Be(result);
        }

        private static string GetCorrectPrintingArray(string name, string type)
        {
            return $"{nameof(Library)}\r\n\t{name} = {type}\r\n\t\t" +
                   $"{nameof(Book)}\r\n\t\t\t" +
                   $"{nameof(Book.Name)} = MyBook\r\n\t\t\t{nameof(Book.Author)} = Alex\r\n\t\t" +
                   $"{nameof(Book)}\r\n\t\t\t" +
                   $"{nameof(Book.Name)} = 1\r\n\t\t\t{nameof(Book.Author)} = John\r\n";
        }

        private string GetCorrectPrintingConfig(string type, int level, string id, string name, 
            string birthDate, string parent = "null\r\n", string child = null)
        {
            var indentation = new string('\t', level);
            return $"{type}\r\n" +
                   (!string.IsNullOrEmpty(id) ? $"{indentation}{nameof(person.Id)} = {id}" : "") +
                   (!string.IsNullOrEmpty(name) ? $"{indentation}{nameof(person.Name)} = {name}" : "") +
                   (!string.IsNullOrEmpty(child) ? $"{indentation}{nameof(person.Parent.Child)} = {child}" : "") +
                   (!string.IsNullOrEmpty(birthDate) ? $"{indentation}{nameof(person.BirthDate)} = {birthDate}" : "") +
                   (!string.IsNullOrEmpty(parent) ? $"{indentation}{nameof(person.Parent)} = {parent}" : "");
        }
    }
}