using System;
using System.Collections.Generic;
using System.Globalization;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests;
using Printer;

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
            person = fixture.Build<Person>()
                .Without(p => p.Father)
                .Without(p => p.Son)
                .Create();
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
                                              $"\tAge = {person.Age}\n" +
                                              "\tFather = null\n" +
                                              "\tSon = null\n");
        }

        [Test]
        public void ExcludeField_should_excludeExpectedFields()
        {
            var sut = ObjectPrinter.For<Person>()
                .Exclude(p => p.Id)
                .PrintToString(person);

            var expectedSerialisedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {person.Height}\n" +
                                           $"\tAge = {person.Age}\n" +
                                           $"\tBirthDate = {person.BirthDate}\n" +
                                           "\tFather = null\n" +
                                           "\tSon = null\n";

            sut.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }

        [Test]
        public void ExcludeProperty_should_excludeExpectedProperty()
        {
            var sut = ObjectPrinter.For<Person>()
                .Exclude(p => p.BirthDate)
                .PrintToString(person);

            var expectedSerialisedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {person.Height}\n" +
                                           $"\tAge = {person.Age}\n" +
                                           "\tFather = null\n" +
                                           "\tSon = null\n" +
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
                                           "\tFather = null\n" +
                                           "\tSon = null\n" +
                                           "\tId = change Guid\n";

            var sut = ObjectPrinter.For<Person>()
                .For<string>(_ => "change string")
                .For<int>(_ => "change int")
                .For<DateTime>(_ => "change DateTime")
                .For<double>(_ => "change double")
                .For<Guid>(_ => "change Guid")
                .PrintToString(person);


            sut.Replace("\r", "").ToLowerInvariant()
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }

        [Test]
        public void PrintToString_objectHasCyclicReferences_notBeStackOverflow()
        {
            var father = fixture.Build<Person>()
                .Without(p => p.Father)
                .Without(p => p.Son)
                .Create();

            father.Son = person;
            person.Father = father;
            var printer = ObjectPrinter.For<Person>();
            var print = new Action(() => { printer.PrintToString(person, int.MaxValue); });

            print.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void PrintToString_objectHasCyclicReferences_shouldBeExpected()
        {
            var father = fixture.Build<Person>()
                .Without(p => p.Father)
                .Without(p => p.Son)
                .Create();

            father.Son = person;
            person.Father = father;
            var expectedSerializedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {person.Height}\n" +
                                           $"\tAge = {person.Age}\n" +
                                           $"\tBirthDate = {person.BirthDate}\n" +
                                           "\tFather = Person\n" +
                                           $"\t\tName = {father.Name}\n" +
                                           $"\t\tHeight = {father.Height}\n" +
                                           $"\t\tAge = {father.Age}\n" +
                                           $"\t\tBirthDate = {father.BirthDate}\n" +
                                           "\t\tFather = null\n" +
                                           $"\t\tSon = {nameof(person)}({person.GetHashCode()})\n" +
                                           $"\t\tId = {father.Id}\n\n" +
                                           "\tSon = null\n" +
                                           $"\tId = {person.Id}\n";

            var sut = ObjectPrinter.For<Person>()
                .PrintToString(person);

            sut.Replace("\r", "").ToLowerInvariant()
                .Should()
                .Be(expectedSerializedPerson.ToLowerInvariant());
        }

        [Test]
        public void StringOfField_should_changeFieldSerialization()
        {
            var expectedSerializedPerson = "Person\n" +
                                           $"\tName = {person.Name}\n" +
                                           $"\tHeight = {person.Height}\n" +
                                           $"\tAge = {person.Age}\n" +
                                           $"\tBirthDate = {person.BirthDate}\n" +
                                           "\tFather = null\n" +
                                           "\tSon = null\n" +
                                           $"\tId = changed id: {person.Id}\n";

            var sut = ObjectPrinter.For<Person>()
                .ForMember(p => p.Id)
                .SetCustomSerializing(g => $"changed id: {g}")
                .Apply()
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
                                           $"\tAge = changed age: {person.Age}\n" +
                                           $"\tBirthDate = {person.BirthDate}\n" +
                                           "\tFather = null\n" +
                                           "\tSon = null\n" +
                                           $"\tId = {person.Id}\n";

            var sut = ObjectPrinter.For<Person>()
                .ForMember(p => p.Age).SetCustomSerializing(a => $"changed age: {a}").Apply()
                .PrintToString(person);

            sut.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerializedPerson.ToLowerInvariant());
        }

        [Test]
        public void LengthOfString_should_ChangeLength()
        {
            var source = "123456789";
            var expected = "12";

            var sut = ObjectPrinter.For<string>()
                .For<string>().LengthOfString(2).Apply()
                .PrintToString(source);
            sut.Should().Be(expected);
        }

        [Test]
        public void PrintToString_should_printingIEnumerable()
        {
            var expected = "[ru],[fi],[ar],[da],[en],[zh],[es],[de],[el],";
            var sut = ObjectPrinter.For<IEnumerable<string>>().PrintToString(Cultures);

            sut.Should().Be(expected);
        }

        [Test]
        public void PrintingToString_should_printingDictionary()
        {
            var source = new Dictionary<int, string> { { 1, "один" }, { 2, "два" } };
            var expected = "{1 : один}\n{2 : два}\n";

            var sut = ObjectPrinter.For<IDictionary<int, string>>().PrintToString(source);

            sut.Should().Be(expected);
        }

        [Test]
        public void PrintingToString_enumerableWithCustomSerialized_itemsShouldBeChanged()
        {
            var expected = "[],[],[],[],[],[],[],[],[],";
            var sut = ObjectPrinter.For<IEnumerable<string>>()
                .For<string>(_ => "")
                .PrintToString(Cultures);

            sut.Should().Be(expected);
        }

        [Test]
        public void PrintingToString_dictionaryWithCustomSerialized_keysShouldBeChanged()
        {
            var source = new Dictionary<int, string> { { 1, "один" }, { 2, "два" } };
            var expected = "{100 : один}\n{200 : два}\n";

            var sut = ObjectPrinter.For<IDictionary<int, string>>()
                .For<int>(x => (x * 100).ToString())
                .PrintToString(source);

            sut.Should().Be(expected);
        }
        
        [Test]
        public void PrintingToString_dictionaryWithCustomSerialized_valuesShouldBeChanged()
        {
            var source = new Dictionary<int, string> { { 1, "один" }, { 2, "два" } };
            var expected = "{1 : 'один'}\n{2 : 'два'}\n";

            var sut = ObjectPrinter.For<IDictionary<int, string>>()
                .For<string>(x => $"'{x}'")
                .PrintToString(source);

            sut.Should().Be(expected);
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
                                           "\tFather = null\n" +
                                           "\tSon = null\n" +
                                           $"\tId = {person.Id}\n";

            var sut = ObjectPrinter
                .For<Person>()
                .For<DateTime>().SetCulture(culture).Apply()
                .For<double>().SetCulture(culture).Apply()
                .For<int>().SetCulture(culture).Apply()
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
                                           "\tFather = null\n" +
                                           "\tSon = null\n" +
                                           $"\tId = {person.Id}\n";

            var sut = ObjectPrinter
                .For<Person>()
                .For<DateTime>()
                .SetCulture(culture)
                .Apply()
                .PrintToString(person);

            sut.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }
    }
}