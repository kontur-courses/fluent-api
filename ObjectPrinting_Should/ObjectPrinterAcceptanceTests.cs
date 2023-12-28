using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        [SetUp]
        public void Setup()
        {
            person = new Person
            {
                Id = new Guid(),
                Name = "Ilya",
                Age = 20,
                Height = 181.5,
            };
        }

        [Test]
        public void PrintToString_ShouldReturnsString()
        {
            var str = ObjectPrinter.For<Person>().PrintToString(person);

            str.Should().BeOfType<string>();
        }

        [Test]
        public void PrintToString_WhenExcludedTypesAreSpecified()
        {
            var str = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .PrintToString(person);

            str.Should().NotContain(nameof(person.Id)).And.NotContain($"{person.Id}");
        }

        [Test]
        public void PrintToString_WhenExcludedPropertiesAreSpecified()
        {
            var str = ObjectPrinter.For<Person>()
                .Excluding(person => person.Name)
                .PrintToString(person);

            str.Should().NotContainAll(nameof(person.Name)).And.NotContain(person.Name);
        }

        [Test]
        public void PrintToString_WhenTheSerializationMethodForTheTypeIsSpecified()
        {
            var str = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(i => i.ToString("X"))
                .PrintToString(person);

            str.Should().ContainAll(new[] { nameof(person.Age), $"{person.Age.ToString("X")}" });
        }

        [Test]
        public void PrintToString_WhenTheSerializationMethodForThePropertyIsSpecified()
        {
            var str = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .Using(name => $"{name} - is my name!")
                .PrintToString(person);

            str.Should().ContainAll(new[] { nameof(person.Name), $"{person.Name} - is my name!"});
        }

        [TestCase("ru-RU")]
        [TestCase("en-US")]
        public void PrintToString_WhenCultureIsSpecifiedForType(string cultureType)
        {
            var culture = new CultureInfo(cultureType);

            var str = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(culture)
                .PrintToString(person);

            str.Should().ContainAll(new[] { nameof(person.Height), $"{person.Height.ToString(culture)}" });
        }

        [TestCase("Leonardo", 1, "L")]
        [TestCase("Michelangelo", 2, "Mi")]
        [TestCase("Raphael", 3, "Rap")]
        [TestCase("Donatello", 4, "Dona")]
        public void PrintToString_WhenStringIsClipped(
    string name, int maxLength, string expected)
        {
            var newPerson = person = new Person
            {
                Id = new Guid(),
                Name = name,
                Age = 20,
                Height = 181.5,
            };

            var str = ObjectPrinter.For<Person>()
                .Printing(person => person.Name)
                .TrimmedToLength(maxLength)
                .PrintToString(person);

            str.Should().Contain(expected).And.NotContain(name);
        }

        [Test]
        public void PrintToString_SerializesCyclicReferences()
        {
            var firstPerson = new Person() { Name = "Ilya", Age = 21, Height = 234.34 };
            var secondPerson = new Person() { Name = "Kirill", Friend = firstPerson, Age = 19, Height = 32.54 };
            firstPerson.Friend = secondPerson;

            Action action = () => ObjectPrinter.For<Person>().PrintToString(firstPerson);

            action.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void PrintToString_SerializesArray()
        {
            var testArray = new[] { 1, 2, 3, 4, 5 };

            var str = ObjectPrinter.For<int[]>().PrintToString(testArray);

            str.Should().Contain("[ 1 2 3 4 5 ]");
        }

        [Test]
        public void PrintToString_SerializesList()
        {
            var testList = new List<string> { "1", "2", "3", "4", "5" };

            var str = ObjectPrinter.For<List<string>>().PrintToString(testList);

            str.Should().Contain("[ 1 2 3 4 5 ]");
        }

        [Test]
        public void PrintToString_SerializesDictionary()
        {
            var testDict = new Dictionary<string, double> { { "Первый ключ", 1 }, { "Второй ключ", 2 } };
            var values = testDict.Values.Select(value => value.ToString()).ToList();
            var keys = testDict.Keys.ToList();

            var str = ObjectPrinter.For<Dictionary<string, double>>().PrintToString(testDict);

            str.Should().ContainAll(new[] { values[0], values[1], keys[0], keys[1]});
        }

        [Test]
        public void PrintToString_SerializesArray_WhenInClass()
        {
            person.Grades = new[] { 1.5, 2.4, 3.3, 4.2, 5.1 };

            var str = ObjectPrinter.For<Person>().PrintToString(person);

            str.Should().ContainAll(new[] {nameof(person.Grades), "[ 1,5 2,4 3,3 4,2 5,1 ]"});
        }
    }
}