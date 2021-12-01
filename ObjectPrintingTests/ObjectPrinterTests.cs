using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;
        private readonly Guid defaultGuid = Guid.NewGuid();
        private List<string> keyWords;

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                FullName = new FullName("Alex", "Smith"),
                Age = 19, Height = 180.2, Weight = 60.5, Id = defaultGuid
            };
            keyWords = new List<string>
            {
                "Alex", "Smith", "19", "180,2", defaultGuid.ToString(),
                "Public", "92"
            };
        }

        [Test]
        public void PrinterShould_PrintSameStrings_FromExtensionAndObjPrinter()
        {
            var printer = ObjectPrinter.For<Person>();

            var fromPrinter = printer.PrintToString(person);
            var fromExtension = person.PrintToString();

            fromExtension.Should().Be(fromPrinter);
        }
        [Test]
        public void PrinterShould_PrintSameStrings_FromExtensionAndObjPrinter_WhenUseSameConfigs()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Field)
                .WithDefaultCulture(CultureInfo.InvariantCulture);

            var fromPrinter = printer.PrintToString(person);
            var fromExtension = person.PrintToString(printer => printer
                .Excluding(p => p.Field)
                .WithDefaultCulture(CultureInfo.InvariantCulture));

            fromExtension.Should().Be(fromPrinter);
        }

        [Test]
        public void PrinterShould_PrintAllFieldsAndProperties()
        {
            var res = person.PrintToString();

            res.Should().ContainAll(keyWords);
        }

        [Test]
        public void PrinterShould_NotPrintPrivateFieldsAndProperties()
        {
            var res = person.PrintToString();

            res.Should().NotContain("private");
        }

        [Test]
        public void PrinterShould_NotPrintExcludedMembers()
        {
            var removedWords = new List<string>
            {
                "19"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_NotPrintExcludedTypes()
        {
            var removedWords = new List<string>
            {
                "Alex",
                "Smith",
                "Public"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_UseAlternativeSerializationForType()
        {
            var removedWords = new List<string>
            {
                "19",
                "92"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("94");
            keyWords.Add("21");
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => (x + 2).ToString());

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_UseAlternativeSerializationForMember()
        {
            var removedWords = new List<string>
            {
                "Alex",
                "Smith",
                "92"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("alex smith");
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.FullName)
                .Using(n => n.Name.ToLower() + " " + n.Surname.ToLower());

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_UseDefaultCulture()
        {
            var removedWords = new List<string>
            {
                "180,2",
                "60,5"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("180.2");
            keyWords.Add("60.5");
            var printer = ObjectPrinter.For<Person>()
                .WithDefaultCulture(CultureInfo.InvariantCulture);

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_UseSelectedCulture()
        {
            var removedWords = new List<string>
            {
                "180,2"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("180.2");
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Height).Using(CultureInfo.InvariantCulture);

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_UseDefaultAndSelectedCultureBoth()
        {
            var removedWords = new List<string>
            {
                "60,5"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("60.5");
            var printer = ObjectPrinter.For<Person>()
                .WithDefaultCulture(CultureInfo.InvariantCulture)
                .Printing(p => p.Height).Using(CultureInfo.CurrentCulture);

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_TrimMemberToLen()
        {
            var removedWords = new List<string>
            {
                "Alex",
                "Smith",
                "Public"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("Al");
            keyWords.Add("Sm");
            keyWords.Add("Pu");
            var printer = ObjectPrinter.For<Person>()
                .WithDefaultCutToLength(2);

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_TrimAllMembersToLen_WhenUseDefaultTrim()
        {
            var removedWords = new List<string>
            {
                "Alex"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("A");
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.FullName.Name)
                .TrimmedToLength(1);

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_TrimMembersToDefaultAndSelectedLenBoth()
        {
            var removedWords = new List<string>
            {
                "Alex",
                "Smith",
                "Public"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("A");
            keyWords.Add("Smit");
            keyWords.Add("Pu");
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.FullName.Name)
                .TrimmedToLength(1)
                .Printing(p => p.FullName.Surname)
                .TrimmedToLength(4)
                .WithDefaultCutToLength(2);

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_FindCyclicReferences()
        {
            person.AnotherPerson = person;
            keyWords.Add("The reference is cyclical");
                
            var res = person.PrintToString();

            res.Should().ContainAll(keyWords);
        }

        [Test]
        public void PrinterShould_NotFPrintCyclicReferences_WhenObjectNotCyclicReferenced()
        {
            person.AnotherPerson = new Person();

            var res = person.PrintToString();

            res.Should().ContainAll(keyWords);
            res.Should().NotContain("The reference is cyclical");
        }

        [Test]
        public void PrinterShould_PrintCollections()
        {
            var listAndArrayKeyWords = new List<string>
            {
                "102", "15", "47"
            };
            var dictKeyWords = new List<string>
            {
                "8", "15", "eight", "fifteen"
            };
            
            var collection = new DefaultCollection();

            collection.PrintToString(conf => conf
                    .Excluding(p => p.array)
                    .Excluding(p => p.dict))
                .Should().ContainAll(listAndArrayKeyWords);

            collection.PrintToString(conf => conf
                    .Excluding(p => p.list)
                    .Excluding(p => p.dict))
                .Should().ContainAll(listAndArrayKeyWords);

            collection.PrintToString(conf => conf
                    .Excluding(p => p.list)
                    .Excluding(p => p.array))
                .Should().ContainAll(dictKeyWords);
        }

        private class DefaultCollection
        {
            public readonly List<int> list = new List<int>
            {
                102, 15, 47
            };

            public int[] array => list.ToArray();

            public Dictionary<int, string> dict = new Dictionary<int, string>
            {
                {8, "eight"},
                {15, "fifteen"}
            };
        }

        private void CheckThatStringContainsKeyWordsExceptOfRemoved(string result, List<string> removed)
        {
            result.Should().ContainAll(keyWords);
            result.Should().NotContainAll(removed);
        }
    }
}