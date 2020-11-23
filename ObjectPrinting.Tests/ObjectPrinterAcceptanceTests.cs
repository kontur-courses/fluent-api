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

        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", BirthDate = new DateTime(2001, 2, 3)};
        }

        [Test]
        public void PrintToString_SerializedWithNewIndentation_WhenChangeIndentation()
        {
            var result = ObjectPrinter.PrintToString(person, x => x.ChangeIndentation(" "));

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    "00000000-0000-0000-0000-000000000000\r\n",
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n",
                    "null\r\n",
                    null,
                    " "));
        }

        [Test]
        public void PrintToString_SerializedWithNewSeparator_WhenChangeSeparator()
        {
            var result = ObjectPrinter.PrintToString(person, x => x.ChangeSeparatorBetweenNameAndValue(":"));

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    "00000000-0000-0000-0000-000000000000\r\n",
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n",
                    "null\r\n",
                    null,
                    "\t",
                    ":"));
        }

        [Test]
        public void PrintToString_SerializedWithoutInt_WhenExcludingGuid()
        {
            var result = ObjectPrinter.PrintToString(person, x => x.Excluding<Guid>());

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    null,
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n"));
        }

        [Test]
        public void PrintToString_SerializedWithChangedGuid_WhenPrintingGuid()
        {
            var result = ObjectPrinter.PrintToString(person, x => x.Printing<Guid>().Using(i => "Guid"));

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    "Guid",
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n"));
        }

        [Test]
        public void PrintToString_SerializedWithCulturedDateTime_WhenPrintingDateTimeWithCulture()
        {
            var result = ObjectPrinter.PrintToString(person, x => x.Printing<DateTime>().Using(CultureInfo.InvariantCulture));

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person), 
                    1,
                    "00000000-0000-0000-0000-000000000000\r\n", 
                    "Alex\r\n",
                    "02/03/2001 00:00:00\r\n"));
        }

        [Test]
        public void PrintToString_SerializedWithChangedMemberParent_WhenPrintingParentAsMember()
        {
            var result = ObjectPrinter.PrintToString(person, x => x.Printing(p => p.Parent).Using(p => "!Parent!"));

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    "00000000-0000-0000-0000-000000000000\r\n",
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n",
                    "!Parent!"));
        }

        [Test]
        public void PrintToString_SerializedWithCutString_WhenPrintingWithCutting()
        {
            var result = ObjectPrinter.PrintToString(person, x => x.Printing(p => p.Name).TrimmedToLength(3));

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    "00000000-0000-0000-0000-000000000000\r\n",
                    "Ale",
                    "03.02.2001 0:00:00\r\n"));
        }

        [Test]
        public void PrintToString_SerializedWithoutMemberId_WhenExcludingId()
        {
            var result = ObjectPrinter.PrintToString(person, x => x.Excluding(p => p.Id));

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1, 
                    null, 
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n"));
        }

        [Test]
        public void PrintToString_SerializedWithFoundCycle_WhenCycleBetweenObjectsDifferentTypes()
        {
            person.Parent = new Parent
            {
                Name = "John",
                Child = person,
                BirthDate = new DateTime(1975, 2, 3)
            };

            var result = ObjectPrinter.PrintToString(person, x => x);

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Person),
                    1,
                    "00000000-0000-0000-0000-000000000000\r\n", 
                    "Alex\r\n",
                    "03.02.2001 0:00:00\r\n",
                    GetCorrectPrintingConfig(
                        nameof(Parent),
                        2,
                        "00000000-0000-0000-0000-000000000000\r\n", 
                        "John\r\n",
                        "03.02.1975 0:00:00\r\n",
                        "null\r\n", 
                        "cycle\r\n")));
        }

        [Test]
        public void PrintToString_SerializedWithFoundCycle_WhenCycleBetweenObjectsSameTypes()
        {
            var parent = new Parent {BirthDate = new DateTime(1975, 2, 3)};
            parent.Parent = parent;

            var result = ObjectPrinter.PrintToString(parent, x => x);

            result.Should().Be(
                GetCorrectPrintingConfig(
                    nameof(Parent),
                    1,
                    "00000000-0000-0000-0000-000000000000\r\n",
                    "null\r\n",
                    "03.02.1975 0:00:00\r\n",
                    "cycle\r\n", 
                    "null\r\n"));
        }

        [Test]
        public void PrintToString_SerializedWithArray_WhenPrintingArray()
        {
            var library = new Library();

            var result = ObjectPrinter.PrintToString(library, x => x.Excluding<List<Book>>()
                .Excluding<Dictionary<string, Book>>());

            result.Should().Be(
                GetCorrectPrintingArray(nameof(Library.BooksArray), library.BooksArray.GetType().Name));
        }

        [Test]
        public void PrintToString_SerializedWithList_WhenPrintingList()
        {
            var library = new Library();

            var result = ObjectPrinter.PrintToString(library,
                x => x.Excluding<Book[]>().Excluding<Dictionary<string, Book>>());

            result.Should().Be(
                GetCorrectPrintingArray(nameof(Library.BooksList), library.BooksList.GetType().Name));
        }

        [Test]
        public void PrintToString_SerializedWithDictionary_WhenPrintingDictionary()
        {
            var library = new Library();
            var serializedObject = ObjectPrinter.PrintToString(library,
                x => x.Excluding<Book[]>().Excluding<List<Book>>());

            var dict = library.BooksDictionary.First();
            var result = $"{nameof(Library)}\r\n\t{nameof(Library.BooksDictionary)} = " +
                         $"{library.BooksDictionary.GetType().Name}\r\n\t\t" +
                         $"{dict.GetType().Name}\r\n\t\t\t" +
                         $"{nameof(dict.Key)} = Alex\r\n\t\t\t" +
                         $"{nameof(dict.Value)} = {nameof(Book)}\r\n\t\t\t\t" +
                         $"{nameof(Book.Name)} = MyBook\r\n\t\t\t\t{nameof(Book.Author)} = Alex\r\n";

            serializedObject.Should().Be(result);
        }


        [Test]
        public void PrintToString_ThrowException_WhenPrintingIncorrectExpression()
        {
            Assert.Throws<InvalidCastException>(() => ObjectPrinter.PrintToString(person,
                x => x.Printing(y => y.Name + 2).Using(CultureInfo.InvariantCulture)));
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
            string birthDate, string parent = "null\r\n", string child = null,
            string indentation = "\t", string sep = "=")
        {
            var indent = string.Concat(Enumerable.Repeat(indentation, level));
            return $"{type}\r\n" +
                   (!string.IsNullOrEmpty(id) ? $"{indent}{nameof(person.Id)} {sep} {id}" : "") +
                   (!string.IsNullOrEmpty(name) ? $"{indent}{nameof(person.Name)} {sep} {name}" : "") +
                   (!string.IsNullOrEmpty(child) ? $"{indent}{nameof(person.Parent.Child)} {sep} {child}" : "") +
                   (!string.IsNullOrEmpty(birthDate) ? $"{indent}{nameof(person.BirthDate)} {sep} {birthDate}" : "") +
                   (!string.IsNullOrEmpty(parent) ? $"{indent}{nameof(person.Parent)} {sep} {parent}" : "");
        }
    }
}