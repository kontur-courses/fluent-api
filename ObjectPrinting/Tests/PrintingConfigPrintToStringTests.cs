using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    class PrintingConfigPrintToStringTests
    {
        private Person person;
        private PersonForTests personForTests;

        [SetUp]
        public void CreatePerson()
        {
            person = new Person { Name = "Alex", Height = 175, Age = 19 };
            personForTests = new PersonForTests { Name = "Alex", Height = 175, Age = 19 };
        }

        [Test]
        public void ShouldReturnCorrectResult_IfNoConfiguration()
        {
            var printingResult = person.PrintToString();

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ShouldReturnResultWithoutType_IfExcludingType()
        {
            var printingResult = person.PrintToString(c => c.Excluding<Guid>());

            printingResult.Should().Be("Person\r\n\tName = Alex\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ShouldReturnResultWithoutProperty_IfExcludingProperty()
        {
            var printingResult = person.PrintToString(c => c.Excluding(p => p.Age));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175\r\n");
        }

        [Test]
        public void ShouldReturnCorrectResult_IfAlternativeSerializationForType()
        {
            var printingResult = person.PrintToString(c => c.Printing<int>().Using(n => (n * 10).ToString()));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175\r\n\tAge = 190\r\n");
        }

        [Test]
        public void ShouldReturnCorrectResult_IfAlternativeSerializationForProperty()
        {
            var printingResult = person.PrintToString(c => c.Printing(p => p.Name).Using(n => "Alexander Bell"));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alexander Bell\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ShouldReturnCorrectResult_IfTrimStringProperty()
        {
            var printingResult = person.PrintToString(c => c.Printing(p => p.Name).TrimmedToLength(2));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Al\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ShouldAddCommaSeparatorInDouble_IfCurrentCulture()
        {
            person.Height = 175.5;
            var printingResult = person.PrintToString(c => c.Printing<double>().Using(new CultureInfo("ru-RU")));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175,5\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ShouldAddPointSeparatorInDouble_IfInvariantCulture()
        {
            person.Height = 175.5;
            var printingResult = person.PrintToString(c => c.Printing<double>().Using(CultureInfo.InvariantCulture));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175.5\r\n\tAge = 19\r\n");
        }

        [Test]
        public void ShouldReturnCorrectResult_IfCollection()
        {
            var collection = new [] { 1, 2 };
            var printingResult = collection.PrintToString();

            printingResult.Should().Be("Int32[]\r\n\tElement 0: 1\r\n\tElement 1: 2\r\n");
        }

        [Test]
        public void ShouldReturnCorrectResult_IfCollectionHaveReferenceTypeElements()
        {
            var collection = new List<PersonForTests> { personForTests, new PersonForTests { Name = "Albert", Age = 25, Height = 185 } };
            var printingResult = collection.PrintToString();

            printingResult.Should().Contain("List")
                .And.Contain("\tElement 0: PersonForTests\r\n")
                .And.Contain("\t\tName = Alex\r\n\t\tHeight = 175\r\n\t\tParent = null\r\n\t\tAge = 19\r\n")
                .And.Contain("\tElement 1: PersonForTests\r\n")
                .And.Contain("\t\tName = Albert\r\n\t\tHeight = 185\r\n\t\tParent = null\r\n\t\tAge = 25\r\n");
        }

        [Test]
        public void ShouldReturnCorrectResult_IfDictionary()
        {
            var collection = new Dictionary<string, int>
            {
                {"hello", 1}, 
                {"world", 2}
            };
            var printingResult = collection.PrintToString();

            printingResult.Should().Contain("\tKey 0: hello\r\n\tValue 0: 1\r\n\tKey 1: world\r\n\tValue 1: 2\r\n");
        }

        [Test]
        public void ShouldReturnCorrectResult_IfDictionaryIsElementOfCollection()
        {
            var firstDictionary = new Dictionary<string, int>
            {
                {"hello", 1},
                {"world", 2}
            };
            var secondDictionary = new Dictionary<string, int>
            {
                {"Earth", 3},
                {"Mars", 4}
            };
            var collection = new [] { firstDictionary, secondDictionary };
            var printingResult = collection.PrintToString();

            printingResult.Should().Contain("Dictionary`2[]\r\n")
                .And.Contain("\tElement 0: Dictionary`2")
                .And.Contain("\t\tKey 0: hello\r\n\t\tValue 0: 1\r\n\t\tKey 1: world\r\n\t\tValue 1: 2\r\n")
                .And.Contain("\tElement 1: Dictionary`2")
                .And.Contain("\t\tKey 0: Earth\r\n\t\tValue 0: 3\r\n\t\tKey 1: Mars\r\n\t\tValue 1: 4\r\n");
        }

        [Test]
        public void ShouldReturnCorrectResult_IfCollectionIsElementOfDictionary()
        {
            var firstCollection = new[] { 1, 2 };
            var secondCollection = new[] { 3, 4 };
            var dictionary = new Dictionary<int[], int[]>
            {
                { firstCollection, secondCollection },
                { secondCollection, firstCollection }
            };
            var printingResult = dictionary.PrintToString();

            printingResult.Should().Contain("Dictionary`2\r\n")
                .And.Contain("\tKey 0: Int32[]\r\n\t\tElement 0: 1\r\n\t\tElement 1: 2\r\n")
                .And.Contain("\tValue 0: Int32[]\r\n\t\tElement 0: 3\r\n\t\tElement 1: 4\r\n")
                .And.Contain("\tKey 1: Int32[]\r\n\t\tElement 0: 3\r\n\t\tElement 1: 4\r\n")
                .And.Contain("\tValue 1: Int32[]\r\n\t\tElement 0: 1\r\n\t\tElement 1: 2\r\n");
        }

        [Test]
        public void ShouldReturnCorrectResult_IfDictionaryHaveReferenceTypeKeysAndValues()
        {
            var secondPersonForTests = new PersonForTests {Name = "Albert", Age = 25, Height = 185};
            var collection = new Dictionary<PersonForTests, PersonForTests>
            {
                {personForTests, secondPersonForTests},
                {secondPersonForTests, personForTests}
            };
            var printingResult = collection.PrintToString();

            printingResult.Should().Contain("Dictionary")
                .And.Contain(
                    "\tKey 0: PersonForTests\r\n\t\tName = Alex\r\n\t\tHeight = 175\r\n\t\tParent = null\r\n\t\tAge = 19\r\n")
                .And.Contain(
                    "\tValue 0: PersonForTests\r\n\t\tName = Albert\r\n\t\tHeight = 185\r\n\t\tParent = null\r\n\t\tAge = 25\r\n")
                .And.Contain(
                    "\tKey 1: PersonForTests\r\n\t\tName = Albert\r\n\t\tHeight = 185\r\n\t\tParent = null\r\n\t\tAge = 25\r\n")
                .And.Contain(
                    "\tValue 1: PersonForTests\r\n\t\tName = Alex\r\n\t\tHeight = 175\r\n\t\tParent = null\r\n\t\tAge = 19\r\n");
        }

        [Test]
        public void ShouldNotAddPrivateField_IfObjHavePrivateField()
        {
            var printingResult = personForTests.PrintToString();

            printingResult.Should().NotContain("id");
        }

        [Test]
        public void ShouldAddPublicField_IfObjHavePublicField()
        {
            var printingResult = personForTests.PrintToString();

            printingResult.Should().Contain("Age");
        }

        [Test]
        public void ShouldСonsiderCyclicReference_IfObjHaveCyclicReference()
        {
            personForTests.Parent = personForTests;
            var printingResult = personForTests.PrintToString();

            printingResult.Should().Contain("Cycle reference");
        }

        [Test]
        public void ShouldСonsiderCyclicReference_IfObjHaveCyclicReferenceThroughOneField()
        {
            personForTests.Parent = new PersonForTests { Name = "Albert", Parent = personForTests };
            var printingResult = personForTests.PrintToString();

            printingResult.Should().Contain("Cycle reference");
        }

        [Test]
        public void ShouldСonsiderNestingLevel_IfNestingLevelMoreThanFive()
        {
            var currentParent = personForTests;
            for (var i = 0; i < 6; i++)
            {
                var newPerson = new PersonForTests { Name = "Alex", Height = 175 + i + 1, Age = 19 };
                newPerson.Parent = currentParent;
                currentParent = newPerson;
            }
            var printingResult = currentParent.PrintToString();

            printingResult.Should().Contain("Nesting level is exceeded");
        }

        [Test]
        public void ShouldСonsiderNull_IfPropertyIsNull()
        {
            personForTests.Name = null;
            var printingResult = personForTests.PrintToString();

            printingResult.Should().Be("PersonForTests\r\n\tName = null\r\n\tHeight = 175\r\n\tParent = null\r\n\tAge = 19\r\n");
        }
    }
}
