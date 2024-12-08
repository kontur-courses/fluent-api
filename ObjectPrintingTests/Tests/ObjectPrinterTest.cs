using FluentAssertions;
using System.Globalization;
using ObjectPrinting.Utilits.Strings;
using ObjectPrinting.Utilits.Objects;
using ObjectPrinting.Utilits.Configs;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person? _person;

        [SetUp]
        public void SetUp()
        {
            _person = new Person()
            {
                Id = new Guid(),
                FirstName = "Ivan",
                SecondName = "Ivanov",
                Height = 180.2,
                Age = 25,
            };
        }

        [Test]
        public void ObjectPrinter_PrintsCorrectlyByDefault()
        {
            var expected = GetBasicPersonString(_person!, 0);

            var printer = ObjectPrinter.For<Person>();
            var actual = printer.PrintToString(_person!);

            actual.Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_PrintsCorrectly_WithTypeExcluding()
        {
            var excluded = new string[]
            {
                $"{nameof(_person.FirstName)} = {_person!.FirstName}{Environment.NewLine}",
                $"{nameof(_person.SecondName)} = {_person.SecondName}{Environment.NewLine}"
            };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();
            var actual = printer.PrintToString(_person);

            actual.Should().NotContainAll(excluded);
        }

        [Test]
        public void ObjectPrinter_PrintsCorrectly_WithPropertyExcluding()
        {
            var excluded = $"{nameof(_person.Age)} = {_person!.Age}{Environment.NewLine}";

            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Age);
            var actual = printer.PrintToString(_person);

            actual.Should().NotContain(excluded);
        }

        [Test]
        public void ObjectPrinter_PrintsCorrectly_WithAlternativeSerializationForType()
        {
            var excluded = new string[]
            {
                $"{nameof(_person.FirstName)} = {_person!.FirstName}{Environment.NewLine}",
                $"{nameof(_person.SecondName)} = {_person.SecondName}{Environment.NewLine}"
            };
            var included = new string[]
            {
                $"{nameof(_person.FirstName)} = Sir {_person.FirstName}{Environment.NewLine}",
                $"{nameof(_person.SecondName)} = Sir {_person.SecondName}{Environment.NewLine}"
            };

            var printer = ObjectPrinter.For<Person>()
                .Printing<string>()
                .Using(x => $"Sir {x}");
            var actual = printer.PrintToString(_person);

            actual.Should().NotContainAll(excluded);
            actual.Should().ContainAll(included);
        }

        [Test]
        public void ObjectPrinter_PrintsCorrectly_WithAlternativeSerializationForProperty()
        {
            var excluded = $"{nameof(_person.FirstName)} = " +
                $"{_person!.FirstName}{Environment.NewLine}";
            var included = $"{nameof(_person.FirstName)} = Sir " +
                $"{_person.FirstName}{Environment.NewLine}";

            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.FirstName)
                .Using(x => $"Sir {x}");
            var actual = printer.PrintToString(_person);

            actual.Should().NotContain(excluded);
            actual.Should().Contain(included);
        }

        [TestCase(4)]
        public void ObjectPrinter_PrintsCorrectly_WithTrimmedEnd(int length)
        {
            var excluded = $"{nameof(_person.SecondName)} = " +
                $"{_person!.SecondName}{Environment.NewLine}";
            var included = $"{nameof(_person.SecondName)} = " +
                $"{_person.SecondName.Substring(0, length)}{Environment.NewLine}";

            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.SecondName)
                .TrimEnd(length);
            var actual = printer.PrintToString(_person);

            actual.Should().NotContain(excluded);
            actual.Should().Contain(included);
        }

        [Test]
        public void ObjectPrinter_PrintsCorrectly_WithCultureInfo()
        {
            var culture = new CultureInfo("en-US");
            var excluded = $"{nameof(_person.Height)} = " +
                $"{_person!.Height}{Environment.NewLine}";
            var included = $"{nameof(_person.Height)} = " +
                $"{_person.Height.ToString(null, culture)}{Environment.NewLine}";

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(culture);
            var actual = printer.PrintToString(_person);

            actual.Should().NotContain(excluded);
            actual.Should().Contain(included);
        }

        [Test]
        public void ObjectExtensions_PrintsCorrectlyByDefault()
        {
            var expected = GetBasicPersonString(_person!, 0);

            var actual = _person.PrintToString();

            actual.Should().Be(expected);
        }

        [Test]
        public void ObjectExtensions_PrintsCorrectlyWithConfig()
        {
            var excluded = $"{nameof(_person.Age)} = " +
                $"{_person!.Age}{Environment.NewLine}";

            var actual = _person.PrintToString(
                s => s.Excluding(p => p.Age));

            actual.Should().NotContain(excluded);
        }

        [Test]
        public void ObjectPrinter_PrintsCorrectly_ArrayProperty()
        {
            _person!.FavoriteNumbers = new int[]
            {
                7,
                21,
                100
            };

            var expected = $"{nameof(_person.FavoriteNumbers)} = " +
                $"\r\n\t{{" +
                $"\r\n\t\t{_person.FavoriteNumbers[0]}" +
                $"\r\n\t\t{_person.FavoriteNumbers[1]}" +
                $"\r\n\t\t{_person.FavoriteNumbers[2]}" +
                $"\r\n\t}}";

            var printer = ObjectPrinter.For<Person>();
            var actual = printer.PrintToString(_person);

            actual.Should().Contain(expected);
        }

        [Test]
        public void ObjectPrinter_PrintsCorrectly_ListProperty()
        {
            _person!.Familiars = new List<Person>()
            {
                new Person()
                {
                    FirstName = "Familiar 1 Name",
                    SecondName = "Familiar 1 SecondName"
                },
                new Person()
                {
                    FirstName = "Familiar 1 Name",
                    SecondName = "Familiar 2 SecondName"
                }
            };

            var excluded = new List<string>();
            foreach (var familiar in _person.Familiars)
            {
                excluded.Add(GetBasicPersonString(familiar, 2));
            }

            var printer = ObjectPrinter.For<Person>();
            var actual = printer.PrintToString(_person);

            actual.Should().ContainAll(excluded);
        }

        [Test]
        public void ObjectPrinter_PrintsCorrectly_DictionaryProperty()
        {
            _person!.PetNames = new Dictionary<string, string>()
            {
                { "Dog", "Rex" },
                { "Cat", "Cleo" }
            };

            var excluded = new List<string>();
            foreach (var pair in _person.PetNames)
            {
                excluded.Add($"{pair.Key} = {pair.Value}");
            }

            var printer = ObjectPrinter.For<Person>();
            var actual = printer.PrintToString(_person);

            actual.Should().ContainAll(excluded);
        }

        [Test]
        public void ObjectPrinter_ThrowsOverflowException_OnCycle()
        {
            _person!.BestFriend = _person;
            Assert.Throws<OverflowException>(() => _person.PrintToString());
        }

        [Test]
        public void ObjectPrinter_PrintToString_ThrowsNullException_RecivingNull()
        {
            _person = null;
            Assert.Throws<ArgumentNullException>(() => _person.PrintToString());
        }

        [TestCase(5)]
        public void ObjectPrinter_PrintsCorrectly_WithMethodChaining(int length)
        {
            var expected = $"{_person!.GetType().Name}" +
                $"\r\n\t{nameof(_person.FirstName)} " +
                $"= Formating for FirstName {_person.FirstName}" +
                $"\r\n\t{nameof(_person.SecondName)} = {_person.SecondName.Substring(0, length)}" +
                $"\r\n\t{nameof(_person.Height)} = " +
                $"{_person.Height.ToString(null, new CultureInfo("en-US"))}" +
                $"\r\n\t{nameof(_person.FavoriteNumbers)} = null" +
                $"\r\n\t{nameof(_person.Familiars)} = null" +
                $"\r\n\t{nameof(_person.PetNames)} = null" +
                $"\r\n\t{nameof(_person.BestFriend)} = null";

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(x => $"Formating for int {x}")
                .Printing<double>().Using(new CultureInfo("en-US"))
                .Printing(x => x.FirstName).Using(x => $"Formating for FirstName {x}")
                .Printing(x => x.SecondName).TrimEnd(5)
                .Excluding(x => x.Age);
            var actual = printer.PrintToString(_person!);

            actual.Should().Be(expected);
        }

        private string GetBasicPersonString(Person person, int nestingLevel)
            => $"{StringUtilits.GetIdentation(nestingLevel)}{person.GetType().Name}" +
                $"\r\n{StringUtilits.GetIdentation(nestingLevel + 1)}" +
                $"{nameof(person.Id)} = {person.Id}" +
                $"\r\n{StringUtilits.GetIdentation(nestingLevel + 1)}" +
                $"{nameof(person.FirstName)} = {person.FirstName}" +
                $"\r\n{StringUtilits.GetIdentation(nestingLevel + 1)}" +
                $"{nameof(person.SecondName)} = {person.SecondName}" +
                $"\r\n{StringUtilits.GetIdentation(nestingLevel + 1)}" +
                $"{nameof(person.Height)} = {person.Height}" +
                $"\r\n{StringUtilits.GetIdentation(nestingLevel + 1)}" +
                $"{nameof(person.Age)} = {person.Age}" +
                $"\r\n{StringUtilits.GetIdentation(nestingLevel + 1)}" +
                $"{nameof(person.FavoriteNumbers)} = null" +
                $"\r\n{StringUtilits.GetIdentation(nestingLevel + 1)}" +
                $"{nameof(person.Familiars)} = null" +
                $"\r\n{StringUtilits.GetIdentation(nestingLevel + 1)}" +
                $"{nameof(person.PetNames)} = null" +
                $"\r\n{StringUtilits.GetIdentation(nestingLevel + 1)}" +
                $"{nameof(person.BestFriend)} = null";
    }
}