using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void Setup()
        {
            var friend = new Person()
            {
                Name = "Ilya",
                Age = 50,
                Height = 111.2
            };

            person = new Person
            {
                Id = new Guid(),
                Name = "Ivan",
                Age = 33,
                Height = 200.1,
                Friend = friend,
            };
        }

        [Test]
        public void PrintToString_ShouldReturnsString()
        {
            var result = ObjectPrinter.For<Person>().PrintToString(person);

            result.Should().BeOfType<string>();
        }

        [Test]
        public void PrintToString_WithExcludedTypes()
        {
            var result = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .PrintToString(person);

            result.Should().NotContain(nameof(person.Id));
            result.Should().NotContain($"{person.Id}");
        }

        [Test]
        public void PrintToString_WithExcludedProperties()
        {
            var result = ObjectPrinter.For<Person>()
                .Exclude(person => person.Name)
                .PrintToString(person);

            result.Should().NotContainAll(nameof(person.Name));
            result.Should().NotContain(person.Name);
        }

        [Test]
        public void PrintToString_WithCustomTypeSerrializer()
        {
            var result = ObjectPrinter.For<Person>()
                .Print<int>().Using(i => i.ToString("X"))
                .PrintToString(person);

            result.Should().ContainAll(new[] { nameof(person.Age), $"{person.Age.ToString("X")}" });
        }

        [Test]
        public void PrintToString_WithCustomPropertySerrializer()
        {
            var result = ObjectPrinter.For<Person>()
                .Print(p => p.Name)
                .Using(name => $"{name} - is my name")
                .PrintToString(person);

            result.Should().ContainAll(new[] { nameof(person.Name), $"{person.Name} - is my name" });
        }

        [TestCase("ru-RU")]
        [TestCase("en-US")]
        public void PrintToString_WithCultureType(string cultureType)
        {
            var culture = new CultureInfo(cultureType);

            var result = ObjectPrinter.For<Person>()
                .Print<double>()
                .Using(culture)
                .PrintToString(person);

            result.Should().ContainAll(new[] { nameof(person.Height), $"{person.Height.ToString(culture)}" });
        }

        [TestCase("Leonardo", 1, "L")]
        [TestCase("Michelangelo", 2, "Mi")]
        [TestCase("Raphael", 3, "Rap")]
        [TestCase("Donatello", 4, "Dona")]
        public void PrintToString_WithTrimLengthForString(string name, int maxLength, string expected)
        {
            person = new Person
            {
                Id = new Guid(),
                Name = name,
                Age = 20,
                Height = 181.5,
            };

            var result = ObjectPrinter.For<Person>()
                .Print(person => person.Name)
                .TrimmedToLength(maxLength)
                .PrintToString(person);

            result.Should().Contain(expected);
            result.Should().NotContain(name);
        }

        [Test]
        public void PrintToString_SerializesArray()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };

            var result = ObjectPrinter.For<int[]>().PrintToString(arr);

            result.Should().Contain("[ 1 2 3 4 5 ]");
        }

        [Test]
        public void PrintToString_SerializesDictionary()
        {
            var dict = new Dictionary<string, double>
            {
                { "первый", 1 },
                { "это второй", 2 }
            };

            var values = dict.Values.Select(value => value.ToString()).ToList();
            var keys = dict.Keys.ToList();

            var str = ObjectPrinter.For<Dictionary<string, double>>().PrintToString(dict);

            str.Should().ContainAll(values.Concat(keys));
        }

        [Test]
        public void PrintToString_Immutable()
        {
            var printer1 = ObjectPrinter.For<Person>().Print(x => x.Name).Using(x => "Ilya");

            var printer2 = printer1.Print(x => x.Name).Using(x => "Home");

            var result1 = printer1.PrintToString(person);
            var result2 = printer2.PrintToString(person);

            result1.Should().NotContain("Home");
            result1.Should().Contain("Ilya");

            result2.Should().NotContain("Ilya");
            result2.Should().Contain("Home");
        }

        [Test]
        public void PrintTostring_WhenUsingExtensionMethod()
        {
            var result = person.PrintToString();

            var fields = new[] {
                $"Name = {person.Name}",
                $"Id = {person.Id}",
                $"Age = {person.Age}",
                $"Height = {person.Height}",
            };

            result.Should().BeOfType<string>();

            result.Should().ContainAll(fields);
            result.Should().ContainAll(person.Friend.PrintToString().Split('\n'));
        }
    }
}
