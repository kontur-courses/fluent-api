using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests;
using Printer;
using AutoFixture;

namespace PrinterTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private readonly Fixture fixture = new();

        private static readonly string[] Cultures =
        {
            "ru", "fi", "ar", "da", "en", "zh", "es", "de", "el"
        };

        [SetUp]
        public void PersonCreator()
        {
            person = fixture.Create<Person>();
        }

        [Test]
        public void Exclude_should_excludeExpectedTypes()
        {
            var sut = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .Exclude<DateTime>()
                .PrintToString(person);

            sut.Replace("\r", "").Should().Be("Person\n" +
                                              $"\tName = {person.Name}\n" +
                                              $"\tHeight = {person.Height}\n" +
                                              $"\tAge = {person.Age}\n");
        }

        [Test]
        public void ExcludeField_should_excludeExpectedFields()
        {
            var sut = ObjectPrinter.For<Person>()
                .ExcludeField(nameof(Person.Id))
                .PrintToString(person);

            var expectedSerialisedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {person.Height}\n" +
                                           $"\tAge = {person.Age}\n" +
                                           $"\tBirthDate = {person.BirthDate}\n";

            sut.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }

        [Test]
        public void ExcludeProperty_should_excludeExpectedProperty()
        {
            var sut = ObjectPrinter.For<Person>()
                .ExcludeProperty(nameof(Person.BirthDate))
                .PrintToString(person);

            var expectedSerialisedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {person.Height}\n" +
                                           $"\tAge = {person.Age}\n" +
                                           $"\tId = {person.Id}\n";

            sut.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }

        [Test]
        public void StringOf_should_changeTypeSerialization()
        {
            var expectedSerialisedPerson = "Person\n" +
                                           "\tName = change string\n" +
                                           "\tHeight = change double\n" +
                                           "\tAge = change int\n" +
                                           "\tBirthDate = change DateTime\n" +
                                           "\tId = change Guid\n";

            var sut = ObjectPrinter.For<Person>()
                .StringOf<string>(_ => "change string")
                .StringOf<int>(_ => "change int")
                .StringOf<DateTime>(_ => "change DateTime")
                .StringOf<double>(_ => "change double")
                .StringOf<Guid>(_ => "change Guid")
                .PrintToString(person);


            sut.Replace("\r", "").ToLowerInvariant()
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }

        [Test]
        public void StringOfField_should_changeFieldSerialization()
        {
            var expectedSerializedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {person.Height}\n" +
                                           $"\tAge = {person.Age}\n" +
                                           $"\tBirthDate = {person.BirthDate}\n" +
                                           $"\tId(Guid) = {person.Id}\n";

            var sut = ObjectPrinter.For<Person>()
                .StringOfField(nameof(Person.Id), (fi, res) => $"{fi.Name}({fi.FieldType.Name}) = {res}")
                .PrintToString(person);

            sut.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerializedPerson.ToLowerInvariant());
        }

        [Test]
        public void StringOfProperty_should_changePropertySerialization()
        {
            var expectedSerializedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {person.Height}\n" +
                                           $"\tAge(int32) = {person.Age}\n" +
                                           $"\tBirthDate = {person.BirthDate}\n" +
                                           $"\tId = {person.Id}\n";

            var sut = ObjectPrinter.For<Person>()
                .StringOfProperty(nameof(Person.Age), 
                    (pi, val) => $"{pi.Name}({pi.PropertyType.Name}) = {val}")
                .PrintToString(person);

            sut.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerializedPerson.ToLowerInvariant());
        }

        [TestCaseSource(nameof(Cultures))]
        public void WithCulture_forGlobalChangeCulture_shouldChangeAllObjects(string cultureTag)
        {
            var culture = new CultureInfo(cultureTag);
            person.Height = 170.5;
            var expectedHeight = person.Height.ToString(culture);
            var expectedBirthDate = person.BirthDate.ToString(culture);
            var expectedSerialisedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {expectedHeight}\n" +
                                           $"\tAge = {person.Age.ToString(culture)}\n" +
                                           $"\tBirthDate = {expectedBirthDate}\n" +
                                           $"\tId = {person.Id}\n";

            var sut = ObjectPrinter
                .For<Person>()
                .WithCulture(culture)
                .PrintToString(person);

            sut.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }

        [TestCaseSource(nameof(Cultures))]
        public void WithCulture_forChangeCultureOnlyBirthDate_shouldNotChangeCultureForAnother(string cultureTag)
        {
            var culture = new CultureInfo(cultureTag);
            person.Height = 170.5;
            var expectedBirthDate = person.BirthDate.ToString(culture);
            var expectedSerialisedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {person.Height}\n" +
                                           $"\tAge = {person.Age}\n" +
                                           $"\tBirthDate = {expectedBirthDate}\n" +
                                           $"\tId = {person.Id}\n";

            var sut = ObjectPrinter
                .For<Person>()
                .WithCultureFor<DateTime>(culture)
                .PrintToString(person);

            sut.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }
    }
}