using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Age = 20, Height = 170.5, Name = "Alex", Surname = "Johnson", University = "UrFU"};
        }


        [Test]
        public void Excluding_ShouldExcludeCertainTypeFromSerialization_WhenArgumentIsType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();

            printer.PrintToString(person)
                .Should()
                .NotContain(nameof(person.Age))
                .And
                .NotContain("20");
        }

        [Test]
        public void Excluding_ShouldNotExcludePropertiesWithOtherTypes_WhenArgumentIsType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();

            printer.PrintToString(person)
                .Should()
                .Contain(nameof(person.Height))
                .And
                .Contain(nameof(person.Id))
                .And
                .Contain(nameof(person.Name));
        }

        [Test]
        public void PrintingAndUsing_ShouldChangeWayOfSerializingType_WhenInputInUsingIsFunction()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(number => "X");

            printer.PrintToString(person)
                .Should()
                .Contain("X")
                .And
                .Contain(nameof(person.Height));
        }

        [Test]
        public void PrintingAndUsing_ShouldNotChangeWayOfSerializingOtherTypes_WhenInputInUsingIsFunction()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(number => "X");

            printer.PrintToString(person)
                .Should()
                .Contain(person.Name)
                .And
                .Contain(person.Age.ToString());
        }

        [TestCaseSource(nameof(UsingShouldChangeNumberCultureWhenInputIsCultureInfoTestCases))]
        public void PrintingAndUsing_ShouldChangeNumberCulture_WhenInputInUsingIsCultureInfo(CultureInfo cultureInfo)
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(cultureInfo);

            printer.PrintToString(person)
                .Should()
                .Contain(person.Height.ToString(cultureInfo));
        }

        private static IEnumerable UsingShouldChangeNumberCultureWhenInputIsCultureInfoTestCases
        {
            get
            {
                yield return new TestCaseData(CultureInfo.InvariantCulture).SetName("invariant culture");
                yield return new TestCaseData(new CultureInfo("en-GB")).SetName("en-GB culture");
                yield return new TestCaseData(new CultureInfo("ru-RU")).SetName("ru-RU culture");
            }
        }

        [Test]
        public void PrintingAndUsing_ShouldChangeWayOfPropertySerialization_WhenInputInPrintingIsPropertyGet()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .Using(name => $"{name}!");

            printer.PrintToString(person)
                .Should()
                .Contain($"{person.Name}!");
        }

        [Test]
        public void PrintingAndUsing_ShouldChangeWayOfSerializationOnlyForSelectedProperty_WhenInputInPrintingIsPropertyGet()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .Using(name => $"{name}!");

            printer.PrintToString(person)
                .Should()
                .NotContain($"{person.Surname}!");
        }

        [Test]
        public void PrintingAndTrimmedToLength_ShouldTrimSelectedProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Surname)
                .TrimmedToLength(5);

            printer.PrintToString(person)
                .Should()
                .Contain(person.Surname.Substring(0, 5))
                .And
                .NotContain(person.Surname);
        }

        [Test]
        public void PrintingAndTrimmedToLength_ShouldTrimOnlySelectedProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(2);

            printer.PrintToString(person)
                .Should()
                .Contain(person.Surname);
        }

        [Test]
        public void TrimmedToLength_ShouldThrow_OnNegativeMaxLength()
        {
            Action createPrinter = () => ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(-1);

            createPrinter.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Test]
        public void Excluding_ShouldExcludeCertainProperty_WhenArgumentIsPropertyGetter()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);

            printer.PrintToString(person)
                .Should()
                .NotContain(person.Age.ToString());
        }

        [Test]
        public void Excluding_ShouldExcludeOnlyCertainProperty_WhenArgumentIsPropertyGetter()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age)
                .Excluding(p => p.Id)
                .Excluding(p => p.Surname);

            printer.PrintToString(person)
                .Should()
                .Contain(person.Height.ToString())
                .And
                .Contain(person.Name);
        }

        [TestCaseSource(nameof(PrintToStringShouldReturnStringThatContainAllElementsOfCollectionWhenInputIsCollectionTestCases))]
        public void PrintToString_ShouldReturnStringThatContainAllElementsOfCollection_WhenInputIsCollection(ICollection collection)
        {
            var printer = ObjectPrinter.For<ICollection>();

            var result = printer.PrintToString(collection);

            foreach (var element in collection)
            {
                result.Should().Contain(element.ToString());
            }
        }

        private static IEnumerable
            PrintToStringShouldReturnStringThatContainAllElementsOfCollectionWhenInputIsCollectionTestCases
        {
            get
            {
                yield return new TestCaseData(new[] {new string[] {"string1", "string2", "string3"}});
                yield return new TestCaseData(new List<int> {100, 200, 300, 400});
            }
        }

        [Test]
        public void PrintToString_ShouldReturnStringThatContainKeysAndValues_WhenInputIsDictionary()
        {
            var dictionary = new Dictionary<string, int> {{"Alex", 10}, {"Mike", 12}, {"Bob", 40}};
            var printer = ObjectPrinter.For<Dictionary<string, int>>();

            var result = printer.PrintToString(dictionary);

            foreach (var keyValuePair in dictionary)
            {
                result.Should().Contain(keyValuePair.Key)
                    .And.Contain(keyValuePair.Value.ToString());
            }
        }

        [Test]
        public void PrintToString_ShouldReturnCorrect_WhenCollectionIsObjectProperty()
        {
            var obj = new
            {
                Name = "Mike",
                Friends = new[] {"Alex", "Bob"},
                Age = 22
            };
            var printer = ObjectPrinter.For<object>();

            var result = printer.PrintToString(obj);

            foreach (var friend in obj.Friends)
            {
                result.Should().Contain(friend);
            }
        }

        [Test]
        public void PrintToString_ShouldThrow_OnObjectWithCyclicReference()
        {
            var newPerson = new Person {Age = 20, Name = "Bob"};
            newPerson.Child = newPerson;
            var printer = ObjectPrinter.For<Person>();

            Action printToString = () => printer.PrintToString(newPerson);

            printToString.Should().ThrowExactly<InvalidOperationException>();
        }

        [Test]
        public void PrintToString_ShouldThrow_WhenObjectNestingLevelIsMoreThanConfigured()
        {
            var newPerson = new Person {Age = 20, Name = "Bob", Child = person};
            var printer = ObjectPrinter.For<Person>()
                .SetNestingLevel(1);

            Action printToString = () => printer.PrintToString(newPerson);

            printToString.Should().ThrowExactly<InvalidOperationException>();
        }

        [Test]
        public void PrintToString_ShouldSerializeFields()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.PrintToString(person)
                .Should()
                .Contain(person.University)
                .And
                .Contain(nameof(person.University));
        }
    }
}