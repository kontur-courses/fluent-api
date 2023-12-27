using NUnit.Framework;
using FluentAssertions;
using ObjectPrinting;
using System.Globalization;
using ObjectPrinter = ObjectPrinting.ObjectPrinter;
using System;
using System.Collections.Generic;

namespace ObjectPrintingTests
{
    public class ObjectPrintingTest
    {
        [Test]
        public void WhenReachedMaxRecursion_ShouldThrowException()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 180.5, SubPerson = new SubPerson() };
            person.SubPerson.Age = 15;
            person.SubPerson.Person = person;

            var printer = ObjectPrinter.For<Person>();

            printer
                .OnMaxRecursion(_ => throw new ArgumentException());

            Action act = () => { printer.PrintToString(person); };

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void WhenAreNoCircularLinks_ShouldReturnCorrectAnswer()
        {
            var subPerson = new SubPerson();
            var person = new Person() {SubPerson = subPerson};

            var parent = new Parent()
            {
                Person = person,
                SubPerson = subPerson
            };

            var actual = parent
                .CreatePrinter()
                .OnMaxRecursion((_) => "РЕКУРСИЯ")
                .PrintToString(parent);

            actual.Should().Be("Parent\r\n\tPerson = Person\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\tName = null\r\n\t\tHeight = 0\r\n\t\tAge = 0\r\n\t\tSubPerson = SubPerson\r\n\t\t\tPerson = null\r\n\t\t\tAge = 0\r\n\t\tPublicField = null\r\n\tSubPerson = SubPerson\r\n\t\tPerson = null\r\n\t\tAge = 0\r\n");
        }

        [Test]
        public void WhenObjectRefersToItself_ShouldReturnCorrectAnswer()
        {
            var currentObject = new SomethingObject();

            currentObject.ToSameObject = currentObject;

            var actual = currentObject
                .CreatePrinter()
                .OnMaxRecursion((_) => "РЕКУРСИЯ")
                .PrintToString(currentObject);

            actual.Should().Be("SomethingObject\r\n\tToSameObject = РЕКУРСИЯ");
        }

        [Test]
        public void ShouldExcludeMember_WhenItsTypeSpecified()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
            printer.Exclude(p => p.Age)
                .Exclude<double>();

            var actual = printer.PrintToString(person);
            actual.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void ShouldUseTrimming_WhenItsSpecifiedForType()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180 };
            var printer = ObjectPrinter.For<Person>();

            var actual = printer
                .Printing(p => p.Name)
                .Trim(1)
                .And.PrintToString(person);

            actual.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = P\r\n\tHeight = 180\r\n\tAge = 20\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void ShouldSerializeMember_WithGivenFunc()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180 };
            var printer = ObjectPrinter.For<Person>();

            var actual = printer
                .Printing(p => p.Age)
                .Using(age => (age + 1000).ToString())
                .And.PrintToString(person);

            actual.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Petr\r\n\tHeight = 180\r\n\tAge = 1020\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void SetCulture_ShouldAddedCultureInfo()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180.5 };
            var printer = ObjectPrinter.For<Person>();

            var actual = printer
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .And.PrintToString(person);

            actual.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Petr\r\n\tHeight = 180.5\r\n\tAge = 20\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void WhenCyclicLinksWasFound_ShouldPrintWithRecursionLimit()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180, SubPerson = new SubPerson() };
            person.SubPerson.Age = 15;
            person.SubPerson.Person = person;
            var printer = ObjectPrinter.For<Person>();

            var actual = printer
                .PrintToString(person);

            actual.Should().Be(
                "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Petr\r\n\tHeight = 180\r\n\tAge = 20\r\n\tSubPerson = SubPerson\r\n\t\tPerson = Maximum recursion has been reached\r\n\t\tAge = 15\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void WhenPassNull_ShouldReturnNullInString()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.PrintToString(null);

            actual.Should().Be("null\r\n");
        }

        [Test]
        public void WhenPassFinalType_ShouldReturnStringRepresentationOfThisType()
        {
            var printer = ObjectPrinter.For<int>();
            var actual = printer.PrintToString(1);

            actual.Should().Be("1\r\n");
        }

        [Test]
        public void WhenDoCustomSerializeAndTrimString_ShouldReturnCorrectResult()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 160 };
            var printer = ObjectPrinter.For<Person>();

            var actual = printer
                .Printing(p => p.Name)
                .Using(n => n + ":))")
                .Trim(6)
                .And.PrintToString(person);

            actual.Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex:)\r\n\tHeight = 160\r\n\tAge = 19\r\n\tSubPerson = null\r\n\tPublicField = null\r\n");
        }

        [Test]
        public void WhenFieldIsList_ShouldReturnCorrectCollectionSerialization()
        {
            var collections = new CollectionsKeeper();
            collections.stringsList = new List<string>() { "один", "два" };

            var printer = collections.CreatePrinter();

            var actual = printer
                .PrintToString(collections);

            actual.Should().Be("CollectionsKeeper\r\n\tstringsList = List`1\r\n\t\tодин\r\n\t\tдва\r\n\tintsArray = null\r\n\tdictionary = null\r\n");
        }

        [Test]
        public void WhenFieldIsArray_ShouldReturnCorrectCollectionSerialization()
        {
            var collections = new CollectionsKeeper();
            collections.intsArray = new int[] { 1, 2, 3 };

            var printer = collections.CreatePrinter();

            var actual = printer
                .PrintToString(collections);

            actual.Should().Be("CollectionsKeeper\r\n\tstringsList = null\r\n\tintsArray = Int32[]\r\n\t\t1\r\n\t\t2\r\n\t\t3\r\n\tdictionary = null\r\n");
        }

        [Test]
        public void WhenFieldIsDictionary_ShouldReturnCorrectCollectionSerialization()
        {
            var collections = new CollectionsKeeper();
            collections.dictionary = new Dictionary<int, string>() { {1, "один"} , { 2, "два" }};

            var printer = collections.CreatePrinter();

            var actual = printer
                .PrintToString(collections);

            actual.Should().Be("CollectionsKeeper\r\n\tstringsList = null\r\n\tintsArray = null\r\n\tdictionary = Dictionary`2\r\n\t\tKeyValuePair`2\r\n\t\t\tKey = 1\r\n\t\t\tValue = один\r\n\t\tKeyValuePair`2\r\n\t\t\tKey = 2\r\n\t\t\tValue = два\r\n");
        }
    }
}
