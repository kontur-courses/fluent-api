using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;
using ObjectPrintingTests.ClassesForTests;

namespace ObjectPrintingTests
{
    public class ObjectPrinterTests
    {
        private const int DefaultNestingLevel = 10;
        private Person person;
        private Student student;
        private static readonly string NewLine = Environment.NewLine; 

        [SetUp]
        public void SetUp()
        {
            person = new Person(Guid.Empty, "Alex", "Johnson", 170.5, 20);
            student = new Student("Alex", "UrFU");
        }


        [Test]
        public void Excluding_ShouldExcludeCertainTypeFromSerialization_WhenArgumentIsType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();

            var result = printer.PrintToString(person);

            var serializingDictionary = person.GetDefaultPersonSerializingDictionary();
            serializingDictionary.Remove(nameof(person.Age));
            var expectedResult = TestHelper.GetExpectedResult(typeof(Person), serializingDictionary);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void PrintingAndUsing_ShouldChangeWayOfSerializingType_WhenInputInUsingIsFunction()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(number => "X");

            var result = printer.PrintToString(person);

            var serializingDictionary = person.GetDefaultPersonSerializingDictionary();
            serializingDictionary[nameof(person.Height)] = "X";
            var expectedResult = TestHelper.GetExpectedResult(typeof(Person), serializingDictionary);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestCaseSource(nameof(UsingShouldChangeNumberCultureWhenInputIsCultureInfoTestCases))]
        public void PrintingAndUsing_ShouldChangeDoubleCulture_WhenInputInUsingIsCultureInfo(CultureInfo cultureInfo)
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(cultureInfo);

            var result = printer.PrintToString(person);

            var serializingDictionary = person.GetDefaultPersonSerializingDictionary();
            serializingDictionary[nameof(person.Height)] = person.Height.ToString(cultureInfo);
            var expectedResult = TestHelper.GetExpectedResult(typeof(Person), serializingDictionary);
            result.Should().BeEquivalentTo(expectedResult);
        }

        private static IEnumerable UsingShouldChangeNumberCultureWhenInputIsCultureInfoTestCases
        {
            get
            {
                yield return new TestCaseData(CultureInfo.InvariantCulture).SetName("invariant culture");
                yield return new TestCaseData(new CultureInfo("en-GB")).SetName("en-GB culture");
                yield return new TestCaseData(new CultureInfo("ru-RU")).SetName("ru-RU culture");
                yield return new TestCaseData(new CultureInfo("ps-AF")).SetName("ps-AF culture");
            }
        }

        [Test]
        public void PrintingAndUsing_ShouldChangeWayOfPropertySerialization_WhenInputInPrintingIsPropertyGet()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .Using(name => $"{name}!");

            var result = printer.PrintToString(person);

            var serializingDictionary = person.GetDefaultPersonSerializingDictionary();
            serializingDictionary[nameof(person.Name)] = $"{person.Name}!";
            var expectedResult = TestHelper.GetExpectedResult(typeof(Person), serializingDictionary);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void PrintingAndTrimmedToLength_ShouldTrimSelectedProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Surname)
                .TrimmedToLength(5);

            var result = printer.PrintToString(person);

            var serializingDictionary = person.GetDefaultPersonSerializingDictionary();
            serializingDictionary[nameof(person.Surname)] = person.Surname.Substring(0, 5);
            var expectedResult = TestHelper.GetExpectedResult(typeof(Person), serializingDictionary);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void TrimmedToLength_ShouldThrow_OnNegativeMaxLength()
        {
            Action createPrinter = () => ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(-1);

            createPrinter.Should().ThrowExactly<ArgumentException>();
        }

        [Test]
        public void Excluding_ShouldExcludeCertainProperty_WhenArgumentIsPropertyGetter()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age)
                .Excluding(p => p.Id)
                .Excluding(p => p.Surname);

            var result = printer.PrintToString(person);

            var expectedResult = TestHelper.GetExpectedResult(typeof(Person), new Dictionary<string, string>
            {
                {nameof(person.Name), person.Name},
                {nameof(person.Height), person.Height.ToString()},

            });
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestCaseSource(nameof(PrintToStringShouldReturnStringThatContainAllElementsOfCollectionWhenInputIsCollectionTestCases))]
        public void PrintToString_ShouldReturnStringThatContainAllElementsOfCollection_WhenInputIsCollection(ICollection collection)
        {
            var printer = ObjectPrinter.For<ICollection>();

            var result = printer.PrintToString(collection);

            var expectedResult = TestHelper.GetExpectedResultForCollection(collection);
            result.Should().BeEquivalentTo(expectedResult);
        }

        private static IEnumerable
            PrintToStringShouldReturnStringThatContainAllElementsOfCollectionWhenInputIsCollectionTestCases
        {
            get
            {
                yield return new TestCaseData(new[] {new string[] {"string1", "string2", "string3"}}).SetName(
                    "array of strings");
                yield return new TestCaseData(new List<int> {100, 200, 300, 400}).SetName("list of integers");
            }
        }

        [Test]
        public void PrintToString_ShouldReturnStringThatContainKeysAndValues_WhenInputIsDictionary()
        {
            var dictionary = new Dictionary<string, int> { { "Alex", 10 }, { "Mike", 12 }, { "Bob", 40 } };
            var printer = ObjectPrinter.For<Dictionary<string, int>>();

            var result = printer.PrintToString(dictionary);

            var expectedResult = TestHelper.GetExpectedResultForCollection(dictionary,
                pair =>
                    $"{pair.GetType().Name}{NewLine}\t\t{nameof(pair.Key)} = {pair.Key}{NewLine}\t\t{nameof(pair.Value)} = {pair.Value.ToString()}");
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void PrintToString_ShouldReturnCorrect_WhenCollectionIsObjectProperty()
        {
            var obj = new
            {
                Friends = new[] { "Alex", "Bob" }
            };
            var printer = ObjectPrinter.For<object>();

            var result = printer.PrintToString(obj);

            var expectedResult = TestHelper.GetExpectedResult(obj.GetType(), new Dictionary<string, string>
            {
                {nameof(obj.Friends), TestHelper.GetExpectedResultForCollection(obj.Friends, "\t\t")}
            });
            result.Should().BeEquivalentTo(expectedResult.Substring(0, expectedResult.Length - NewLine.Length));
        }

        [Test]
        public void PrintToString_ShouldReturnStringAccordingToDefaultNestingLevel_OnObjectWithCyclicReference()
        {
            var king = new King(null);
            king.Parent = king;
            var printer = ObjectPrinter.For<King>();

            var result = printer.PrintToString(king);

            var expectedFirstLevels = Enumerable.Range(1, DefaultNestingLevel - 1).Select(i =>
                    string.Concat(Enumerable.Repeat("\t", i)) + $"{nameof(king.Parent)} = King" + NewLine);
            var expectedResult = "King" + NewLine + string.Concat(expectedFirstLevels) +
                                 string.Concat(Enumerable.Repeat("\t", DefaultNestingLevel)) +
                                 $"{nameof(king.Parent)} = {king}" + NewLine;
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void PrintToString_ShouldReturnStringAccordingToConfiguredNestingLevel_WhenObjectNestingLevelIsMoreThanConfigured()
        {
            var obj = new
            {
                Friend = person
            };
            var printer = ObjectPrinter.For<object>()
                .SetNestingLevel(1);

            var result = printer.PrintToString(obj);

            var expectedResult = TestHelper.GetExpectedResult(obj.GetType(), new Dictionary<string, string>
            {
                {nameof(obj.Friend), person.ToString()}
            });
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void PrintToString_ShouldSerializeFields()
        {
            var printer = ObjectPrinter.For<Student>();

            var result = printer.PrintToString(student);

            var expectedResult = TestHelper.GetExpectedResult(typeof(Student), new Dictionary<string, string>
            {
                {nameof(student.Name), student.Name},
                {nameof(student.University), student.University},

            });
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestCaseSource(nameof(UsingShouldChangeNumberCultureWhenInputIsCultureInfoTestCases))]
        public void PrintingAndUsing_ShouldChangeIntCulture_WhenInputInUsingIsCultureInfo(CultureInfo cultureInfo)
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(cultureInfo);

            var result = printer.PrintToString(person);

            var serializingDictionary = person.GetDefaultPersonSerializingDictionary();
            serializingDictionary[nameof(person.Age)] = person.Age.ToString(cultureInfo);
            var expectedResult = TestHelper.GetExpectedResult(typeof(Person), serializingDictionary);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestCaseSource(nameof(UsingShouldChangeNumberCultureWhenInputIsCultureInfoTestCases))]
        public void PrintingAndUsing_ShouldChangeFloatCulture_WhenInputInUsingIsCultureInfo(CultureInfo cultureInfo)
        {
            var point = new
            {
                X = 1.0f,
                Y = 1.1f
            };
            var printer = ObjectPrinter.For<object>()
                .Printing<float>()
                .Using(cultureInfo);

            var result = printer.PrintToString(point);

            var expectedResult = TestHelper.GetExpectedResult(point.GetType(), new Dictionary<string, string>
            {
                {nameof(point.X), point.X.ToString(cultureInfo)},
                {nameof(point.Y), point.Y.ToString(cultureInfo)},

            });
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void Printing_ShouldThrow_WhenInputFunctionIsNotPropertyGetter()
        {
            Action createPrinter = () => ObjectPrinter.For<Person>()
                .Printing(p => "1");

            createPrinter.Should().ThrowExactly<ArgumentException>();
        }

        [Test]
        public void Printing_ShouldThrow_WhenInputFunctionIsNull()
        {
            Action createPrinter = () => ObjectPrinter.For<Person>()
                .Printing<string>(null);

            createPrinter.Should().ThrowExactly<ArgumentException>();
        }

        [Test]
        public void Excluding_ShouldThrow_WhenInputFunctionIsNotPropertyGetter()
        {
            Action createPrinter = () => ObjectPrinter.For<Student>()
                .Printing(s => s.University);

            createPrinter.Should().ThrowExactly<ArgumentException>();
        }

        [Test]
        public void Excluding_ShouldThrow_WhenInputFunctionIsNull()
        {
            Action createPrinter = () => ObjectPrinter.For<Person>()
                .Excluding<Person>(null);

            createPrinter.Should().ThrowExactly<ArgumentException>();
        }
    }
}