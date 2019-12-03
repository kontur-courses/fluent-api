using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests.TestTypes;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class Tests
    {
        private Person person;

        [SetUp]
        public void SetUp() =>
            person = new Person
            {
                Name = "Alex",
                Property = "qwerty",
                Age = 19,
                Height = 232.32432
            };

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

        [Test]
        public void PrintToString_WhenSerialisedObjectHasCyclicDependency_ThrowsApplicationException()
        {
            var cyclicA = new CyclicTypeA();
            var cyclicB = new CyclicTypeB();

            cyclicA.CyclicProperty = cyclicB;
            cyclicB.CyclicProperty = cyclicA;

            Assert.Throws<ApplicationException>(() => cyclicA.PrintToString());
        }

        [Test]
        public void PrintToString_WhenSerialisedObjectHasTwoPropertiesWithTheSameReferenceToObject()
        {
            const string mutualReferenceObject = "";

            person.Name = mutualReferenceObject;
            person.Property = mutualReferenceObject;

            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "Person",
                                                    "\tId = Guid",
                                                    "\tName = ",
                                                    "\tProperty = ",
                                                    "\tHeight = 232.32432",
                                                    $"\tAge = 19{Environment.NewLine}");

            person.PrintToString().Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenSerialisedObjectHasCollection()
        {
            var objectWithCollections = new TypeWithCollections
            {
                Numbers = new[] { 1, 2, 3 },
                Strings = new List<string> { "a", "b", "c" },
                NumberByString = new Dictionary<string, int>
                {
                    ["d"] = 4,
                    ["e"] = 5,
                    ["f"] = 6
                }
            };

            var expectedSerialisation = string.Join(Environment.NewLine,
                                                    "TypeWithCollections",
                                                    "\tNumbers = 1 2 3",
                                                    "\tStrings = a b c",
                                                    $"\tNumberByString = [d]: 4 [e]: 5 [f]: 6{Environment.NewLine}");

            objectWithCollections.PrintToString().Should().Be(expectedSerialisation);
        }

        [Test]
        public void PrintToString_WhenSerialisedObjectHasCollectionOfCollections()
        {
            var objectWithCollectionOfCollections = new TypeWithCollectionOfCollections
            {
                ArrayOfArrays = new[] { new[] { 1, 2 }, new[] { 3, 4 } },

                ListOfLists = new List<List<string>>(new[]
                {
                    new List<string>(new[] { "a", "b" }),
                    new List<string>(new[] { "c", "d" })
                }),

                DictionaryByString = new Dictionary<string, Dictionary<string, int>>
                {
                    ["x"] = new Dictionary<string, int> { ["e"] = 5, ["f"] = 6 },
                    ["y"] = new Dictionary<string, int> { ["g"] = 7, ["h"] = 8 }
                }
            };

            var arrayType = new int[0].GetType();
            var listType = new List<string>().GetType();
            var dictionaryType = new Dictionary<string, int>().GetType();

            var expectedSerialisation =
                string.Join(
                    Environment.NewLine,
                    "TypeWithCollectionOfCollections",
                    $"\tArrayOfArrays = {arrayType}(1 2) {arrayType}(3 4)",
                    $"\tListOfLists = {listType}(a b) {listType}(c d)",
                    $"\tDictionaryByString = [x]: {dictionaryType}([e]: 5 [f]: 6) [y]: {dictionaryType}([g]: 7 [h]: 8)")
              + Environment.NewLine;

            objectWithCollectionOfCollections.PrintToString().Should().Be(expectedSerialisation);
        }
    }
}