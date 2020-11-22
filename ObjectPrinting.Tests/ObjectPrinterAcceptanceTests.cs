using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 19, BirthDate = new DateTime(2001, 2, 3) };
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingInt()
        {
            printer.Excluding<int>();

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                null, null, "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingDouble()
        {
            printer.Excluding<double>();

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", null,
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingGuid()
        {
            printer.Excluding<Guid>();

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, null, "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingDateTime()
        {
            printer.Excluding<DateTime>();

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", null), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingString()
        {
            printer.Excluding<string>();

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", null, "0\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingParent()
        {
            printer.Excluding<Parent>();

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n", null), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingInt()
        {
            printer.Printing<int>().Using(i => (i + 1).ToString());

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "20", "1", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingString()
        {
            printer.Printing<string>().Using(i => i + i);

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "AlexAlex", "0\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingGuid()
        {
            printer.Printing<Guid>().Using(i => "Guid");

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid", "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingParent()
        {
            person.Parent = new Parent();
            printer.Printing<Parent>().Using(i => "!Parent!");

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n", "!Parent!"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingDoubleWithCulture()
        {
            person.Height = 171.5;
            printer.Printing<double>().Using(CultureInfo.InvariantCulture);

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "171.5\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingDateTimeWithCulture()
        {
            printer.Printing<DateTime>().Using(CultureInfo.InvariantCulture);

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", "02/03/2001 00:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingAge()
        {
            printer.Printing(p => p.Age).Using(p => $"!{p}!");

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "!19!", "0\r\n", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingBirthDate()
        {
            printer.Printing(p => p.BirthDate).Using(p => $"!{p}!");

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", "!03.02.2001 0:00:00!"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingParentAsMember()
        {
            printer.Printing(p => p.Parent).Using(p => "!Parent!");

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n", "!Parent!"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingWithCutting()
        {
            printer.Printing(p => p.Name).TrimmedToLength(3);

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Ale", "0\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingAge()
        {
            printer.Excluding(p => p.Age);

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                null, "0\r\n", "03.02.2001 0:00:00\r\n"), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingBirthDate()
        {
            printer.Excluding(p => p.BirthDate);

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", null), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingParentAsMember()
        {
            printer.Excluding(p => p.Parent);

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n", null), printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenCycleBetweenObjectsDifferentTypes()
        {
            person.Parent = new Parent
            {
                Name = "John",
                Age = 45,
                Child = person,
                BirthDate = new DateTime(1975, 2, 3)
            };

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Person), 1, "Guid\r\n", "Alex\r\n", "0\r\n",
                    "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n",
                    GetCorrectPrintingConfig(nameof(Parent), 2, "Guid\r\n", "John\r\n", "0\r\n",
                        "45\r\n", "0\r\n", "03.02.1975 0:00:00\r\n", "null\r\n", "cycle\r\n")),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenCycleBetweenObjectsSameTypes()
        {
            var parent = new Parent { BirthDate = new DateTime(2001, 2, 3) };
            parent.Parent = parent;

            Assert.AreEqual(GetCorrectPrintingConfig(nameof(Parent), 1, "Guid\r\n", "null\r\n", "0\r\n",
                "0\r\n", "0\r\n", "03.02.2001 0:00:00\r\n", "cycle\r\n", "null\r\n"), printer.PrintToString(parent));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingArray()
        {
            var library = new Library();
            var printerLibrary = new PrintingConfig<Library>().Excluding<List<Book>>()
                .Excluding<Dictionary<string, Book>>();

            var result = GetCorrectPrintingArray(nameof(Library.BooksArray), library.BooksArray.GetType().Name);
            Assert.AreEqual(result, printerLibrary.PrintToString(library));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingList()
        {
            var library = new Library();
            var printerLibrary =
                new PrintingConfig<Library>().Excluding<Book[]>().Excluding<Dictionary<string, Book>>();

            var result = GetCorrectPrintingArray(nameof(Library.BooksList), library.BooksList.GetType().Name);
            Assert.AreEqual(result, printerLibrary.PrintToString(library));
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

            Assert.AreEqual(result, printerLibrary.PrintToString(library));
        }

        private static string GetCorrectPrintingArray(string name, string type)
        {
            return $"{nameof(Library)}\r\n\t{name} = {type}\r\n\t\t" +
                   $"{nameof(Book)}\r\n\t\t\t" +
                   $"{nameof(Book.Name)} = MyBook\r\n\t\t\t{nameof(Book.Author)} = Alex\r\n\t\t" +
                   $"{nameof(Book)}\r\n\t\t\t" +
                   $"{nameof(Book.Name)} = 1\r\n\t\t\t{nameof(Book.Author)} = John\r\n";
        }

        private string GetCorrectPrintingConfig(string type, int level, string id, string name, string height,
            string age, string weight, string birthDate, string parent = "null\r\n", string child = null)
        {
            var indentation = new string('\t', level);
            return $"{type}\r\n" +
                   (!string.IsNullOrEmpty(id) ? $"{indentation}{nameof(person.Id)} = {id}" : "") +
                   (!string.IsNullOrEmpty(name) ? $"{indentation}{nameof(person.Name)} = {name}" : "") +
                   (!string.IsNullOrEmpty(height) ? $"{indentation}{nameof(person.Height)} = {height}" : "") +
                   (!string.IsNullOrEmpty(age) ? $"{indentation}{nameof(person.Age)} = {age}" : "") +
                   (!string.IsNullOrEmpty(weight) ? $"{indentation}{nameof(person.Weight)} = {weight}" : "") +
                   (!string.IsNullOrEmpty(child) ? $"{indentation}{nameof(person.Parent.Child)} = {child}" : "") +
                   (!string.IsNullOrEmpty(birthDate) ? $"{indentation}{nameof(person.BirthDate)} = {birthDate}" : "") +
                   (!string.IsNullOrEmpty(parent) ? $"{indentation}{nameof(person.Parent)} = {parent}" : "");
        }
    }
}
