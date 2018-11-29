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
        private string ExpectedAge => $"\tAge = {person.Age}";
        private string ExpectedHeight => $"\tHeight = {person.Height.ToString(CultureInfo.InvariantCulture)}";
        private string ExpectedName => $"\tName = {person.Name}";
        private string ExpectedId => $"\tId = {person.Id}";
        private const string ExpectedChild = "\tChild = null";
        private const string ExpectedParent = "\tParent = null";

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
                "Person", ExpectedId, ExpectedName, ExpectedHeight, ExpectedAge, ExpectedParent, ExpectedChild, "");
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
                $"Person\r\n\tId = {person.Id}\r\n\tName = John\r\n\tHeight = 1.8\r\n\tAge = 18\r\n\tParent = " +
                $"Person\r\n\t\tId = {person.Parent.Id}\r\n\t\tName = Steve\r\n\t\tHeight = 1.9\r\n\t\tAge = 35\r\n\t\tParent = " +
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
                "Person", ExpectedId, ExpectedName, ExpectedHeight, ExpectedParent, ExpectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToUseAlternativeSerializationForTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing<int>().Using(i => "int");

            var expected = string.Join("\r\n",
                "Person", ExpectedId, ExpectedName, ExpectedHeight, "\tAge = int", ExpectedParent, ExpectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToUseProvidedCultureInfo()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing<double>().Using(CultureInfo.GetCultureInfoByIetfLanguageTag("ru"));

            var expected = string.Join("\r\n",
                "Person", ExpectedId, ExpectedName, "\tHeight = 1,8", ExpectedAge, ExpectedParent, ExpectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToUseAlternativeSerializationForProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing(p => p.Id).Using(i => "Guid");

            var expected = string.Join("\r\n",
                "Person", "\tId = Guid", ExpectedName, ExpectedHeight, ExpectedAge, ExpectedParent, ExpectedChild, "");
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
                "Person", ExpectedId, "\tName = Solofonant", ExpectedHeight, ExpectedAge, ExpectedParent, ExpectedChild, "");
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToExcludeProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .ExcludingProperty(p => p.Name);

            var expected = string.Join("\r\n",
                "Person", ExpectedId, ExpectedHeight, ExpectedAge, ExpectedParent, ExpectedChild, "");
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
