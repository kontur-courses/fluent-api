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
        public void PrinterShould_ThrowException_WhenUseNotProperyOrFieldInLambda()
        {
            Assert.Throws<InvalidCastException>(() => ObjectPrinter.For<Person>()
                .Excluding(p => p.GetHashCode()));
        }

        [Test]
        public void PrinterShould_ThrowException_WhenUseNotMemberExpressionInLambda()
        {
            Assert.Throws<InvalidCastException>(() => ObjectPrinter.For<Person>()
                .Excluding(p => 10));
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
        public void PrinterShould_UseLastAlternativeSerialization_WhenChangeSerialization()
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
                .Using(n => n.Name + " " + n.Surname)
                .Printing(p => p.FullName)
                .Using(n => n.Name.ToLower() + " " + n.Surname.ToLower());

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_UseLastAlternativeSerialization_WhenSetSerializationOfMemberAfterType()
        {
            var removedWords = new List<string>
            {
                "19",
                "92"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("age");
            keyWords.Add("{93}");
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(n => $"{{{n + 1}}}")
                .Printing(p => p.Age)
                .Using(n => "age" );

            var res = printer.PrintToString(person);

            CheckThatStringContainsKeyWordsExceptOfRemoved(res, removedWords);
        }

        [Test]
        public void PrinterShould_UseLastAlternativeSerialization_WhenSetSerializationOfTypeAfterMember()
        {
            var removedWords = new List<string>
            {
                "19",
                "92"
            };
            keyWords = keyWords.Except(removedWords).ToList();
            keyWords.Add("{20}");
            keyWords.Add("{93}");
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age)
                .Using(n => "age")
                .Printing<int>()
                .Using(n => $"{{{n + 1}}}");

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
        public void PrinterShould_PrintArray()
        {
            var arrayElements = new string[]
            {
                "102", "15", "47"
            };

            var res = arrayElements.PrintToString();

            res.Should().ContainAll(arrayElements);
        }

        [Test]
        public void PrinterShould_PrintList()
        {
            var listElements = new List<string>
            {
                "102", "15", "47"
            };

            var res = listElements.PrintToString();

            res.Should().ContainAll(listElements);
        }

        [Test]
        public void PrinterShould_PrintDictionary()
        {
            var dict = new Dictionary<int, string>
            {
                {8, "eight"},
                {15, "fifteen"}
            };
            var dictElememts = new List<string>
            {
                "8", "15", "eight", "fifteen"
            };

            var res = dict.PrintToString();

            res.Should().ContainAll(dictElememts);
        }

        [Test]
        public void PrinterShould_PrintInnerCollections()
        {
            var listOfLists = new List<List<string>>
            {
                new List<string>
                {
                    "102", "15", "47"
                },
                new List<string>
                {
                "50", "51", "52"
                }
            };
            var listElements = listOfLists.SelectMany(list => list).ToList();

            var res = listElements.PrintToString();

            res.Should().ContainAll(listElements);
        }

        [Test]
        public void PrinterShould_StopPrintingEndlessIEnumerable()
        {

            var printedElements = GetEndlessIEnumerable().Select(e => e.ToString())
                .Take(100);

            var res = GetEndlessIEnumerable().PrintToString();

            res.Should().ContainAll(printedElements);
            res.Should().Contain("IEnumerable probably endless!");
        }

        private IEnumerable<int> GetEndlessIEnumerable()
        {
            var i = 0;
            while (true)
            {
                yield return i;
                i = ++i % 100;
            }
        }

        private void CheckThatStringContainsKeyWordsExceptOfRemoved(string result, List<string> removed)
        {
            result.Should().ContainAll(keyWords);
            result.Should().NotContainAll(removed);
        }
    }
}