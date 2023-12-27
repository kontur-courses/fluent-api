using NUnit.Framework;
using FluentAssertions;
using ObjectPrinting;
using System.Globalization;
using ObjectPrinter = ObjectPrinting.ObjectPrinter;
using System;
using System.Collections.Generic;
using ObjectPrintingTests.TestHelpers;
using ObjectPrinting.Serialization;
using ObjectPrinting.Solved.Tests;
using Person = ObjectPrintingTests.TestHelpers.Person;

namespace ObjectPrintingTests
{
    public class ObjectPrintingTest
    {
        [Test]
        public void WhenPassComplexObject_ShouldReturnCorrectSerializeString()
        {
            var person = new Person();

            var printer = person.CreatePrinter();

            var actual = printer.PrintToString(person);

            actual.Should().Be($"Person{Environment.NewLine}\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\tName = null{Environment.NewLine}\tHeight = 0{Environment.NewLine}\tAge = 0{Environment.NewLine}\tSubPerson = null{Environment.NewLine}\tPublicField = null{Environment.NewLine}");
        }

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

            actual.Should().Be($"Parent{Environment.NewLine}\tPerson = Person{Environment.NewLine}\t\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\t\tName = null{Environment.NewLine}\t\tHeight = 0{Environment.NewLine}\t\tAge = 0{Environment.NewLine}\t\tSubPerson = SubPerson{Environment.NewLine}\t\t\tPerson = null{Environment.NewLine}\t\t\tAge = 0{Environment.NewLine}\t\tPublicField = null{Environment.NewLine}\tSubPerson = SubPerson{Environment.NewLine}\t\tPerson = null{Environment.NewLine}\t\tAge = 0{Environment.NewLine}");
        }

        [Test]
        public void WhenObjectReferenceToItself_ShouldReturnCorrectAnswer()
        {
            var currentObject = new SomethingObject();
            currentObject.ToSameObject = currentObject;

            var actual = currentObject
                .CreatePrinter()
                .OnMaxRecursion((_) => "РЕКУРСИЯ")
                .PrintToString(currentObject);

            actual.Should().Be($"SomethingObject{Environment.NewLine}\tToSameObject = РЕКУРСИЯ{Environment.NewLine}\tCount = 1{Environment.NewLine}");
        }

        [Test]
        public void WhenItsTypeSpecified_ShouldExcludeMember()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
            printer.Exclude(p => p.Age)
                .Exclude<double>();

            var actual = printer.PrintToString(person);

            actual.Should().Be(
                $"Person{Environment.NewLine}\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\tName = Alex{Environment.NewLine}\tSubPerson = null{Environment.NewLine}\tPublicField = null{Environment.NewLine}");
        }

        [Test]
        public void WhenItsSpecifiedForType_ShouldUseTrimming()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180 };
            var printer = ObjectPrinter.For<Person>();

            var actual = printer
                .Printing(p => p.Name)
                .Trim(1)
                .And.PrintToString(person);

            actual.Should().Be(
                $"Person{Environment.NewLine}\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\tName = P{Environment.NewLine}\tHeight = 180{Environment.NewLine}\tAge = 20{Environment.NewLine}\tSubPerson = null{Environment.NewLine}\tPublicField = null{Environment.NewLine}");
        }

        [Test]
        public void WithGivenFunc_ShouldSerializeMember()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180 };
            var printer = ObjectPrinter.For<Person>();

            var actual = printer
                .Printing(p => p.Age)
                .Using(age => (age + 1000).ToString())
                .And.PrintToString(person);

            actual.Should().Be(
                $"Person{Environment.NewLine}\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\tName = Petr{Environment.NewLine}\tHeight = 180{Environment.NewLine}\tAge = 1020{Environment.NewLine}\tSubPerson = null{Environment.NewLine}\tPublicField = null{Environment.NewLine}");
        }

        [Test]
        public void WhenPassCustomSetCulture_ShouldAddedCultureInfo()
        {
            var person = new Person { Name = "Petr", Age = 20, Height = 180.5 };
            var printer = ObjectPrinter.For<Person>();

            var actual = printer
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .And.PrintToString(person);

            actual.Should().Be(
                $"Person{Environment.NewLine}\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\tName = Petr{Environment.NewLine}\tHeight = 180.5{Environment.NewLine}\tAge = 20{Environment.NewLine}\tSubPerson = null{Environment.NewLine}\tPublicField = null{Environment.NewLine}");
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
                $"Person{Environment.NewLine}\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\tName = Petr{Environment.NewLine}\tHeight = 180{Environment.NewLine}\tAge = 20{Environment.NewLine}\tSubPerson = SubPerson{Environment.NewLine}\t\tPerson = Maximum recursion has been reached{Environment.NewLine}\t\tAge = 15{Environment.NewLine}\tPublicField = null{Environment.NewLine}");
        }

        [Test]
        public void WhenPassNull_ShouldReturnNullInString()
        {
            var printer = ObjectPrinter.For<Person>();
            var actual = printer.PrintToString(null);

            actual.Should().Be($"null{Environment.NewLine}");
        }

        [Test]
        public void WhenPassFinalType_ShouldReturnStringRepresentationOfThisType()
        {
            var printer = ObjectPrinter.For<int>();
            var actual = printer.PrintToString(1);

            actual.Should().Be($"1{Environment.NewLine}");
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

            actual.Should().Be($"Person{Environment.NewLine}\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\tName = Alex:){Environment.NewLine}\tHeight = 160{Environment.NewLine}\tAge = 19{Environment.NewLine}\tSubPerson = null{Environment.NewLine}\tPublicField = null{Environment.NewLine}");
        }

        [Test]
        public void WhenFieldIsList_ShouldReturnCorrectCollectionSerialization()
        {
            var collections = new CollectionsKeeper();
            collections.stringsList = new List<string>() { "один", "два" };

            var printer = collections.CreatePrinter();

            var actual = printer
                .PrintToString(collections);

            actual.Should().Be($"CollectionsKeeper{Environment.NewLine}\tstringsList = List`1{Environment.NewLine}\t\tодин{Environment.NewLine}\t\tдва{Environment.NewLine}\tintsArray = null{Environment.NewLine}\tdictionary = null{Environment.NewLine}");
        }

        [Test]
        public void WhenFieldIsArray_ShouldReturnCorrectCollectionSerialization()
        {
            var collections = new CollectionsKeeper();
            collections.intsArray = new int[] { 1, 2, 3 };

            var printer = collections.CreatePrinter();

            var actual = printer
                .PrintToString(collections);

            actual.Should().Be($"CollectionsKeeper{Environment.NewLine}\tstringsList = null{Environment.NewLine}\tintsArray = Int32[]{Environment.NewLine}\t\t1{Environment.NewLine}\t\t2{Environment.NewLine}\t\t3{Environment.NewLine}\tdictionary = null{Environment.NewLine}");
        }

        [Test]
        public void WhenFieldIsDictionary_ShouldReturnCorrectCollectionSerialization()
        {
            var collections = new CollectionsKeeper();
            collections.dictionary = new Dictionary<int, string>() { {1, "один"} , { 2, "два" }};

            var printer = collections.CreatePrinter();

            var actual = printer
                .PrintToString(collections);

            actual.Should().Be($"CollectionsKeeper{Environment.NewLine}\tstringsList = null{Environment.NewLine}\tintsArray = null{Environment.NewLine}\tdictionary = Dictionary`2{Environment.NewLine}\t\tKeyValuePair`2{Environment.NewLine}\t\t\tKey = 1{Environment.NewLine}\t\t\tValue = один{Environment.NewLine}\t\tKeyValuePair`2{Environment.NewLine}\t\t\tKey = 2{Environment.NewLine}\t\t\tValue = два{Environment.NewLine}");
        }

        [Test]
        public void WhenFieldIsIEnumerable_ShouldReturnCorrectCollectionSerialization()
        {
            IEnumerable<int> enumerable = new[] { 1,2 };

            var printer = enumerable.CreatePrinter();

            var actual = printer
                .PrintToString(enumerable);
            
            actual.Should().Be($"Int32[]{Environment.NewLine}\t1{Environment.NewLine}\t2{Environment.NewLine}");
        }

        [Test]
        public void WhenFieldIsComplexObject_ShouldReturnCorrectCollectionSerialization()
        {
            var person = new Person();
            person.SubPerson = new SubPerson();

            var printer = person.CreatePrinter();

            var actual = printer
                .PrintToString(person);

            actual.Should().Be($"Person{Environment.NewLine}\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\tName = null{Environment.NewLine}\tHeight = 0{Environment.NewLine}\tAge = 0{Environment.NewLine}\tSubPerson = SubPerson{Environment.NewLine}\t\tPerson = null{Environment.NewLine}\t\tAge = 0{Environment.NewLine}\tPublicField = null{Environment.NewLine}");
        }

        [Test]
        public void WhenCollectionElementIsComplexObject_ShouldReturnCorrectCollectionSerialization()
        {
            var listWithComplexObject = new List <Person>() { new Person() };

            var printer = listWithComplexObject.CreatePrinter();

            var actual = printer
                .PrintToString(listWithComplexObject);

            actual.Should().Be($"List`1{Environment.NewLine}\tPerson{Environment.NewLine}\t\tId = 00000000-0000-0000-0000-000000000000{Environment.NewLine}\t\tName = null{Environment.NewLine}\t\tHeight = 0{Environment.NewLine}\t\tAge = 0{Environment.NewLine}\t\tSubPerson = null{Environment.NewLine}\t\tPublicField = null{Environment.NewLine}");
        }

        [Test]
        public void WhenPassDictionaryToPrinter_ShouldReturnThisCollectionInString()
        {
            var dict = new Dictionary<int, string>{{1, "один"} };

            var printer = dict.CreatePrinter();

            var actual = printer.PrintToString(dict);

            actual.Should().Be($"Dictionary`2{Environment.NewLine}\tKeyValuePair`2{Environment.NewLine}\t\tKey = 1{Environment.NewLine}\t\tValue = один{Environment.NewLine}");
        }

        [Test]
        public void WhenPassListToPrinter_ShouldReturnThisCollectionInString()
        {
            var list = new List<float>{1.7f, 6.5f};

            var printer = list.CreatePrinter();

            var actual = printer.PrintToString(list);

            actual.Should().Be($"List`1{Environment.NewLine}\t1,7{Environment.NewLine}\t6,5{Environment.NewLine}");
        }

        [Test]
        public void WhenToSameObjectInTwoFields_ShouldReturnCorrectResultWithoutRecursion()
        {
            var test = new Test();
            var obj = new SomethingObject();

            test.ToSameObject = obj;
            test.ToSameObject2 = obj;

            var printer = test.CreatePrinter();

            var actual = printer.PrintToString(test);

            actual.Should().Be($"Test{Environment.NewLine}\tToSameObject = SomethingObject{Environment.NewLine}\t\tToSameObject = null{Environment.NewLine}\t\tCount = 1{Environment.NewLine}\tToSameObject2 = SomethingObject{Environment.NewLine}\t\tToSameObject = null{Environment.NewLine}\t\tCount = 1{Environment.NewLine}");
        }

        [Test]
        public void WhenPassEmptyCollection_ShouldPrintOnlyCollectionType()
        {
            var list = new List<double>();

            var printer = list.CreatePrinter();

            var actual = printer.PrintToString(list);

            actual.Should().Be($"List`1{Environment.NewLine}");

        }

        [Test]
        public void WhenPassFinalTypeWithCustomSerialization_ShouldReturnCustomSerializationFuncResultString()
        {
            var currentString = new string("abcd");

            var printer = currentString.CreatePrinter();

            var actual = printer
                .Printing<string>()
                .Trim(3)
                .And.PrintToString(currentString);

            actual.Should().Be($"abc{Environment.NewLine}");
        }

        [Test]
        public void WhenPassNullToUsing_ShouldThrowNullReferenceExceprion()
        {
            var currentString = "abc";
            var printer = currentString.CreatePrinter();

            Action actual = () => { printer
                .Printing<string>()
                .Using(null)
                .And.PrintToString(currentString); };

            actual.Should().Throw<ArgumentException>();
        }

        [Test]
        public void WhenPassNullToWrap_ShouldThrowNullReferenceExceprion()
        {
            var currentString = "abc";
            var printer = currentString.CreatePrinter();

            Action actual = () => {
                printer
                    .Printing<string>()
                    .Using(p => currentString + 1)
                    .Wrap(null)
                    .And.PrintToString(currentString);
            };

            actual.Should().Throw<ArgumentException>();
        }
    }
}
