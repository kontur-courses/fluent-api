using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class Tests
    {
        private readonly Person person = new Person
            { Name = "Alex", Age = 19, Height = 232.32432, Property = "qwerty" };

        [Test]
        public void PrintToString_ReturnsSerialisedObject()
        {
            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tId = Guid",
                                                    "\tName = Alex",
                                                    "\tProperty = qwerty",
                                                    "\tHeight = 232.32432",
                                                    $"\tAge = 19{Environment.NewLine}");

            person.PrintToString().Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenExcludedOneProperty()
        {
            string serialisedPerson = person.PrintToString(printingConfig => printingConfig.Excluding(p => p.Name));

            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tId = Guid",
                                                    "\tProperty = qwerty",
                                                    "\tHeight = 232.32432",
                                                    $"\tAge = 19{Environment.NewLine}");

            serialisedPerson.Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenExcludedSomeProperties()
        {
            string serialisedPerson = person.PrintToString(printingConfig => printingConfig
                                                                             .Excluding(p => p.Name)
                                                                             .Excluding(p => p.Age)
                                                                             .Excluding(p => p.Id));


            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tProperty = qwerty",
                                                    $"\tHeight = 232.32432{Environment.NewLine}");

            serialisedPerson.Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenExcludedAllProperties()
        {
            string serialisedPerson = person.PrintToString(printingConfig => printingConfig
                                                                             .Excluding(p => p.Name)
                                                                             .Excluding(p => p.Age)
                                                                             .Excluding(p => p.Id)
                                                                             .Excluding(p => p.Property)
                                                                             .Excluding(p => p.Height));
            var expectedSerialisation = $"Person{Environment.NewLine}";

            serialisedPerson.Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenExcludedType()
        {
            string serialisedPerson = person.PrintToString(printingConfig => printingConfig.Excluding<string>());

            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tId = Guid",
                                                    "\tHeight = 232.32432",
                                                    $"\tAge = 19{Environment.NewLine}");

            serialisedPerson.Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenSetAlternateSerialisationBehaviorForType()
        {
            string serialisedPerson = person.PrintToString(
                printingConfig => printingConfig.Printing<string>()
                                                .Using(s => $"<alternate>{s}</alternate>"));

            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tId = Guid",
                                                    "\tName = <alternate>Alex</alternate>",
                                                    "\tProperty = <alternate>qwerty</alternate>",
                                                    "\tHeight = 232.32432",
                                                    $"\tAge = 19{Environment.NewLine}");

            serialisedPerson.Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenSetAlternateSerialisationBehaviorForProperty()
        {
            string serialisedPerson = person.PrintToString(
                printingConfig => printingConfig.Printing(p => p.Age)
                                                .Using(age => $"<alternate>{age}</alternate>"));

            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tId = Guid",
                                                    "\tName = Alex",
                                                    "\tProperty = qwerty",
                                                    "\tHeight = 232.32432",
                                                    $"\tAge = <alternate>19</alternate>{Environment.NewLine}");

            serialisedPerson.Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenSetAlternateCultureInfoForType()
        {
            string serialisedPerson = person.PrintToString(printingConfig => printingConfig
                                                                             .Printing<double>()
                                                                             .Using(CultureInfo.GetCultureInfo("ru")));
            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tId = Guid",
                                                    "\tName = Alex",
                                                    "\tProperty = qwerty",
                                                    "\tHeight = 232,32432",
                                                    $"\tAge = 19{Environment.NewLine}");

            serialisedPerson.Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenSetTrimmingForAllStringProperties()
        {
            string serialisedPerson = person.PrintToString(printingConfig => printingConfig
                                                                             .Printing<string>()
                                                                             .TrimmedToLength(2));
            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tId = Guid",
                                                    "\tName = Al...",
                                                    "\tProperty = qw...",
                                                    "\tHeight = 232.32432",
                                                    $"\tAge = 19{Environment.NewLine}");

            serialisedPerson.Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenSetTrimmingForConcreteProperty()
        {
            string serialisedPerson = person.PrintToString(printingConfig => printingConfig
                                                                             .Printing(p => p.Property)
                                                                             .TrimmedToLength(2));
            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tId = Guid",
                                                    "\tName = Alex",
                                                    "\tProperty = qw...",
                                                    "\tHeight = 232.32432",
                                                    $"\tAge = 19{Environment.NewLine}");

            serialisedPerson.Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenSetZeroTrimming_ThrowsArgumentException() =>
            Assert.Throws<ArgumentException>(
                () => person.PrintToString(printingConfig => printingConfig
                                                             .Printing(p => p.Property)
                                                             .TrimmedToLength(0)));
    }
}