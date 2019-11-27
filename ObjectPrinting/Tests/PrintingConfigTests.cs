using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    class PrintingConfigTests
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
        public void PrintToString_ShouldReturnCorrectResult_IfNoConfiguration()
        {
            var printingResult = person.PrintToString();

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnResultWithoutType_IfExcludingType()
        {
            var printingResult = person.PrintToString(c => c.Excluding<Guid>());

            printingResult.Should().Be("Person\r\n\tName = Alex\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnResultWithoutProperty_IfExcludingProperty()
        {
            var printingResult = person.PrintToString(c => c.Excluding(p => p.Age));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnCorrectResult_IfAlternativeSerializationForType()
        {
            var printingResult = person.PrintToString(c => c.Printing<int>().Using(n => (n * 10).ToString()));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175\r\n\tAge = 190\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnCorrectResult_IfAlternativeSerializationForProperty()
        {
            var printingResult = person.PrintToString(c => c.Printing(p => p.Name).Using(n => "Alexander Bell"));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alexander Bell\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnCorrectResult_IfTrimStringProperty()
        {
            var printingResult = person.PrintToString(c => c.Printing(p => p.Name).TrimmedToLength(2));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Al\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldAddCommaSeparatorInDouble_IfCurrentCulture()
        {
            person.Height = 175.5;
            var printingResult = person.PrintToString(c => c.Printing<double>().Using(CultureInfo.CurrentCulture));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175,5\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldAddPointSeparatorInDouble_IfInvariantCulture()
        {
            person.Height = 175.5;
            var printingResult = person.PrintToString(c => c.Printing<double>().Using(CultureInfo.InvariantCulture));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175.5\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnCorrectResult_IfCollection()
        {
            var collection = new [] { 1, 2 };
            var printingResult = collection.PrintToString();

            printingResult.Should().Be("Int32[]\r\n\tElement 0: 1\r\n\tElement 1: 2\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnCorrectResult_IfDictionary()
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
        public void PrintToString_ShouldNotAddPrivateField_IfObjHavePrivateField()
        {
            var printingResult = personForTests.PrintToString();

            printingResult.Should().NotContain("id");
        }

        [Test]
        public void PrintToString_ShouldAddPublicField_IfObjHavePublicField()
        {
            var printingResult = personForTests.PrintToString();

            printingResult.Should().Contain("Age");
        }

        [Test]
        public void PrintToString_ShouldСonsiderCycleReference_IfObjHaveCycleReference()
        {
            personForTests.Parent = personForTests;
            var printingResult = personForTests.PrintToString();

            printingResult.Should().Contain("Cycle reference");
        }

        [Test]
        public void PrintToString_ShouldСonsiderNestingLevel_IfNestingLevelMoreThanFive()
        {
            var currentParent = personForTests;
            for (var i = 0; i < 5; i++)
            {
                var newPerson = new PersonForTests { Name = "Alex", Height = 175 + i + 1, Age = 19 };
                newPerson.Parent = currentParent;
                currentParent = newPerson;
            }
            var printingResult = currentParent.PrintToString();

            printingResult.Should().Contain("Nesting level is exceeded");
        }

        [Test]
        public void PrintToString_ShouldСonsiderNull_IfPropertyIsNull()
        {
            personForTests.Name = null;
            var printingResult = personForTests.PrintToString();

            printingResult.Should().Be("PersonForTests\r\n\tName = null\r\n\tHeight = 175\r\n\tParent = null\r\n\tAge = 19\r\n");
        }
    }
}
