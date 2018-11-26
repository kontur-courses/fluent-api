using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person person;
        private string expectedAge => $"\tAge = {person.Age}";
        private string expectedHeight => $"\tHeight = {person.Height}";
        private string expectedName => $"\tName = {person.Name}";
        private const string expectedId = "\tId = Guid";
        private const string expectedChild = "\tChild = null";
        private const string expectedParent = "\tParent = null";

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Height = 1.8,
                Age = 18,
            };
        }

        [Test]
        public void ReturnSameResultForRegularMethodAndExtension()
        {
            var serialization1 = ObjectPrinter.For<Person>().PrintToString(person);
            var serialization2 = person.Print();

            serialization1.Should().Be(serialization2);
        }

        [Test]
        public void ReturnExpectedDefaultSerialization()
        {
            var expected = string.Join("\r\n",
                "Person", expectedId, expectedName, expectedHeight, expectedAge, expectedParent, expectedChild, "");
            var actual = person.Print();

            actual.Should().Be(expected);
        }

        [Test]
        public void WorkCorrectlyWithCyclicLinks()
        {
            var parent = new Person
            {
                Age = 35,
                Height = 1.9,
                Id = Guid.NewGuid(),
                Name = "Steve",
                Child = person
            };
            person.Parent = parent;

            var expected =
                "Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 1,8\r\n\tAge = 18\r\n\tParent = " +
                "Person\r\n\t\tId = Guid\r\n\t\tName = Steve\r\n\t\tHeight = 1,9\r\n\t\tAge = 35\r\n\t\tParent = " +
                "null\r\n\t\tChild = Person (already printed)\r\n\tChild = null\r\n";
            var actual = person.Print();

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToExcludeTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .ExcludingType<int>();

            var expected = string.Join("\r\n",
                "Person", expectedId, expectedName, expectedHeight, expectedParent, expectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToUseAlternativeSerializationForTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing<int>().Using(i => "int");

            var expected = string.Join("\r\n",
                "Person", expectedId, expectedName, expectedHeight, "\tAge = int", expectedParent, expectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToUseProvidedCultureInfo()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing<double>().Using(CultureInfo.GetCultureInfoByIetfLanguageTag("en"));

            var expected = string.Join("\r\n",
                "Person", expectedId, expectedName, "\tHeight = 1.8", expectedAge, expectedParent, expectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToUseAlternativeSerializationForProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing(p => p.Id).Using(i => i.ToString());

            var expected = string.Join("\r\n",
                "Person", $"\tId = {person.Id}", expectedName, expectedHeight, expectedAge, expectedParent, expectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToTrimStringProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing(p => p.Name).TrimmingToLength(10);
            person.Name = "Solofonantenaina Randriamaholison";

            var expected = string.Join("\r\n",
                "Person", expectedId, "\tName = Solofonant", expectedHeight, expectedAge, expectedParent, expectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToExcludeProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .ExcludingProperty(p => p.Name);

            var expected = string.Join("\r\n",
                "Person", expectedId, expectedHeight, expectedAge, expectedParent, expectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToWorkWithCollections()
        {
            var list = new List<int> {1, 2, 3, 4, 5};

            const string expected = "List`1\r\n\t0: 1\r\n\t1: 2\r\n\t2: 3\r\n\t3: 4\r\n\t4: 5\r\n";
            var actual = list.Print();

            actual.Should().Be(expected);
        }
    }
}
