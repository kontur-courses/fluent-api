using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person person;
        private string ExpectedAge => $"Age = {person.Age}";
        private string ExpectedHeight => $"Height = {person.Height.ToString(CultureInfo.InvariantCulture)}";
        private string ExpectedName => $"Name = {person.Name}";
        private string ExpectedId => $"Id = {person.Id}";
        private const string ExpectedChild = "Child = null";
        private const string ExpectedParent = "Parent = null";
        private readonly string newLine = Environment.NewLine;
        private const string Indentation = "\t";

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Height = 1.8,
                Age = 18
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
            var expected = string.Join(newLine + Indentation,
                "Person", ExpectedId, ExpectedName, ExpectedHeight, ExpectedAge, ExpectedParent, ExpectedChild)
                + newLine;
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

            var expectedParent = string.Join(newLine + Indentation + Indentation,
                "Person", $"Id = {person.Parent.Id}", $"Name = {person.Parent.Name}",
                $"Height = {person.Parent.Height.ToString(CultureInfo.InvariantCulture)}",
                $"Age = {person.Parent.Age}", ExpectedParent, "Child = Person (already printed)");
            var expected = string.Join(newLine + Indentation,
                               "Person", ExpectedId, ExpectedName, ExpectedHeight, ExpectedAge,
                               $"Parent = {expectedParent}", ExpectedChild) + newLine;
            var actual = person.Print();

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToExcludeTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .ExcludingType<int>();

            var expected = string.Join(newLine + Indentation,
                "Person", ExpectedId, ExpectedName, ExpectedHeight, ExpectedParent, ExpectedChild)
                + newLine;
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToUseAlternativeSerializationForTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing<int>().Using(i => "int");

            var expected = string.Join(newLine + Indentation,
                "Person", ExpectedId, ExpectedName, ExpectedHeight, "Age = int", ExpectedParent, ExpectedChild)
                + newLine;
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToUseProvidedCultureInfo()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing<double>().Using(CultureInfo.GetCultureInfoByIetfLanguageTag("ru"));

            var expected = string.Join(newLine + Indentation,
                "Person", ExpectedId, ExpectedName, "Height = 1,8", ExpectedAge, ExpectedParent, ExpectedChild)
                + newLine;
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToUseAlternativeSerializationForProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing(p => p.Id).Using(i => "Guid");

            var expected = string.Join(newLine + Indentation,
                "Person", "Id = Guid", ExpectedName, ExpectedHeight, ExpectedAge, ExpectedParent, ExpectedChild)
                + newLine;
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToTrimStringProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Serializing(p => p.Name).TrimmingToLength(10);
            person.Name = "Solofonantenaina Randriamaholison";

            var expected = string.Join(newLine + Indentation,
                "Person", ExpectedId, "Name = Solofonant", ExpectedHeight, ExpectedAge, ExpectedParent, ExpectedChild)
                + newLine;
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToExcludeProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .ExcludingProperty(p => p.Name);

            var expected = string.Join(newLine + Indentation,
                "Person", ExpectedId, ExpectedHeight, ExpectedAge, ExpectedParent, ExpectedChild)
                + newLine;
            var actual = printer.PrintToString(person);

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToWorkWithCollections()
        {
            var list = new List<int> {1, 2, 3, 4, 5};

            var expected = string.Join(newLine + Indentation,
                               "List`1", "0: 1", "1: 2", "2: 3", "3: 4", "4: 5")
                           + newLine;
            var actual = list.Print();

            actual.Should().Be(expected);
        }

        [Test]
        public void BeAbleToPrintFields()
        {
            var fieldPerson = new FieldPerson
            {
                Id = person.Id,
                Name = person.Name,
                Height = person.Height,
                Age = person.Age
            };

            var expected = string.Join(newLine + Indentation,
                               "FieldPerson", ExpectedId, ExpectedName, ExpectedHeight, ExpectedAge, ExpectedParent, ExpectedChild)
                           + newLine;
            var actual = fieldPerson.Print();

            actual.Should().Be(expected);
        }
    }
}
