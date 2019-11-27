using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    class PrintingConfigTests
    {
        private Person person;

        [SetUp]
        public void CreatePerson()
        {
            person = new Person { Name = "Alex", Height = 175, Age = 19 };
        }

        [Test]
        public void PrintToString_ShouldReturnCorrectResult_IfNoConfiguration()
        {
            var printingResult = person.PrintToString();

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnCorrectResult_IfExcludingType()
        {
            var printingResult = person.PrintToString(c => c.Excluding<Guid>());

            printingResult.Should().Be("Person\r\n\tName = Alex\r\n\tHeight = 175\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnCorrectResult_IfExcludingProperty()
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
        public void PrintToString_ShouldReturnCorrectResult_IfCurrentCulture()
        {
            person.Height = 175.5;
            var printingResult = person.PrintToString(c => c.Printing<double>().Using(CultureInfo.CurrentCulture));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175,5\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ShouldReturnCorrectResult_IfInvariantCulture()
        {
            person.Height = 175.5;
            var printingResult = person.PrintToString(c => c.Printing<double>().Using(CultureInfo.InvariantCulture));

            printingResult.Should().Be("Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 175.5\r\n\tAge = 19\r\n");
        }
    }
}
