using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    public class ObjectPrinterTests
    {
        private Person person;
        private string result;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Age = 20, Height = 185.5, Name = "Alex" };
            result = "";
        }

        [Test]
        public void Printer_ShouldPrintEverything_WithoutConfiguration()
        {
            var printer = ObjectPrinter.For<Person>();
            result = printer.PrintToString(person);
            result.Should().Contain("185,5").And.Contain("Alex").And.Contain("20");
        }

        [Test]
        public void Excluding_ShouldExcludeType_OnInt()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();
            result = printer.PrintToString(person);
            result.Should().Contain("185,5").And.Contain("Alex").And.NotContain("20").And.NotContain("Age");
        }

        [Test]
        public void Excluding_ShouldExcludeProperty_OnName()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Name);
            result = printer.PrintToString(person);
            result.Should().Contain("185,5").And.Contain("20").And.NotContain("Alex");
        }

        [Test]
        public void Excluding_ShouldExcludeField_OnWeight()
        {
            person.Weight = 90;
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Weight);
            result = printer.PrintToString(person);
            result.Should().NotContain("Weight").And.NotContain("90");
        }

        [Test]
        public void Printing_ShouldPrintTypeAsSpecified_OnInt()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(i => "abc");
            result = printer.PrintToString(person);
            result.Should().Contain("abc").And.NotContain("20");
        }

        [Test]
        public void Printing_ShouldPrintTypeWithSpecifiedCulture_OnDouble()
        {
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture);
            result = printer.PrintToString(person);
            result.Should().NotContain("185,5").And.Contain("185.5");
        }

        [Test]
        public void Printing_ShouldReturnSecondGivenFormat_OnTwoFormats()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(d => "double")
                .Printing<double>().Using(CultureInfo.InvariantCulture);
            result = printer.PrintToString(person);
            result.Should().NotContain("185,5").And.NotContain("double").And.Contain("185.5");
        }

        [Test]
        public void Printing_ShouldPrintPropertyAsSpecified_OnName()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).Using(n => $"Имя: {n}");
            result = printer.PrintToString(person);
            result.Should().Contain("Имя: Alex").And.NotContain("Name = Alex");
        }

        [Test]
        public void Printing_ShouldPrintFieldAsSpecified_OnMarried()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Married).Using(m => m ? "Married" : "Single");
            result = printer.PrintToString(person);
            result.Should().Contain("Single");
        }

        [Test]
        public void TrimmedToLength_ShouldPrintTrimmedProperty_OnLongProperty()
        {
            person.Name = "abcdefghijklmnop";
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(4);
            result = printer.PrintToString(person);
            result.Should().Contain("abcd").And.NotContain("abcdefghijklmnop");
        }

        [Test]
        public void ExtensionPrintToString_ShouldSerializeAsUsualPrintToString_WithoutConfiguration()
        {
            var printer = ObjectPrinter.For<Person>();
            var expectedResult = printer.PrintToString(person);

            result = person.PrintToString();

            result.Should().Be(expectedResult);
        }

        [Test]
        public void ExtensionPrintToString_ShouldSerializeAsUsualPrintToString_WithConfiguration()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).Using(n => $"Имя: {n}");
            var expectedResult = printer.PrintToString(person);

            result = person.PrintToString(c => c.Printing(p => p.Name).Using(n => $"Имя: {n}"));

            result.Should().Be(expectedResult);
        }

        [Test]
        public void PrintToString_ShouldCorrectlyProcessCyclicReferences()
        {
            var person1 = new Person {Name = "Alice"};
            var person2 = new Person { Name = "Bob", Partner = person1};
            person1.Partner = person2;

            result = person1.PrintToString();

            result.Should().Contain("Partner contains cyclic reference");
        }

        [Test]
        public void PrintToString_ShouldPrintCollections_OnArray()
        {
            person.Marks = new[] {1, 2, 3};
            result = person.PrintToString();
            result.Should().Contain("1").And.Contain("2").And.Contain("3");

        }

        [Test]
        public void PrintToString_ShouldPrintCollections_OnList()
        {
            person.VisitedCountries = new List<string>{"a", "b", "c"};
            result = person.PrintToString();
            result.Should().Contain("a").And.Contain("b").And.Contain("c");
        }

        [Test]
        public void PrintToString_ShouldPrintCollections_OnDictionary()
        {
            person.YearsPerJob = new Dictionary<string, int>
            {
                {"a", 1 },
                {"b", 2 },
                {"c", 3 },
            };
            result = person.PrintToString();
            result.Should().Contain("a = 1").And.Contain("b = 2").And.Contain("c = 3");
        }

        [Test]
        public void PrintToString_ShouldPrintCollections_OnInfiniteEnumerable()
        {
            person.SomeNumbers = GetInfiniteEnumerable();
            result = person.PrintToString();
            result.Should().Contain("1, 1, 1, 1, 1, 1, 1, 1, 1, 1, ...");
        }

        private static IEnumerable<int> GetInfiniteEnumerable()
        {
            while (true)
                yield return 1;
        }

        [Test]
        public void PrintToString_ShouldPrintCollections_OnEmptyEnumerable()
        {
            person.SomeNumbers = new List<int>();
            result = person.PrintToString();
            result.Should().Contain("[]");
        }

        [Test]
        public void PrintToString_ShouldPrintCollections_OnNestedCollections()
        {
            var collection = new List<List<int>>
            {
                new List<int> {1, 2},
                new List<int> {3, 4},
                new List<int> {5, 6}
            };
            result = collection.PrintToString();
            result.Should().Contain("[[1, 2], [3, 4], [5, 6]]");
        }

        [Test]
        public void PrintToString_ShouldPrintCollections_OnNestedCollectionsAsClassMember()
        {
            var collection = new List<List<int>>
            {
                new List<int> {1, 2},
                new List<int> {3, 4},
                new List<int> {5, 6}
            };
            person.SomeNumbersLists = collection;
            result = person.PrintToString();
            result.Should().Contain("[[1, 2], [3, 4], [5, 6]]");
        }

        [Test]
        public void PrintToString_ShouldPrintFields()
        {
            person.Married = true;
            person.Weight = 90;
            result = person.PrintToString();
            result.Should().Contain("Married = True").And.Contain("Weight = 90");
        }
    }
}
