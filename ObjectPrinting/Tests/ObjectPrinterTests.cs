using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person()
            {
                Id = Guid.NewGuid(),
                Name = "Alex",
                Age = 42,
                Height = 179.9,
                Weight = 69.5,
                Occupation = "Idle",
                Child = new Person
                {
                    Id = Guid.NewGuid(),
                    Name = "Max",
                    Age = 24,
                    Height = 175.6,
                    Weight = 74.3,
                    Occupation = "Student"
                }
            };
        }

        [Test]
        public void PrintToString_ShouldReturnsString()
        {
            var str = ObjectPrinter.For<Person>().PrintToString(person);
            str.Should().BeOfType<string>();
        }

        [Test]
        public void PrintToString_ShouldPrintStringCorrectly()
        {
            var str = ObjectPrinter.For<Person>().PrintToString(person);
            str.Should().Contain(
                $"\tId = {person.Id}",
                $"\tName = {person.Name}",
                $"\tAge = {person.Age}",
                $"\tHeight = {person.Height}",
                $"\tWeight = {person.Weight}",
                $"\tOccupation = {person.Occupation}",
                $"\tChild = {person.GetType()}",
                $"\t\tId = {person.Child.Id}",
                $"\t\tName = {person.Child.Name}",
                $"\t\tAge = {person.Child.Age}",
                $"\t\tHeight = {person.Child.Height}",
                $"\t\tWeight = {person.Child.Weight}",
                $"\t\tOccupation = {person.Child.Occupation}",
                $"\t\tChild = {person.Child.Child}"
            );
        }

        [Test]
        public void PrintToString_PropertiesWithExcludedType_ShouldNotPrint()
        {
            var str = ObjectPrinter.For<Person>()
                .Excluding<double>()
                .PrintToString(person);
            str.Should().NotContainAll(new[] { "Height", "Weight" });
        }

        [Test]
        public void PrintToString_PropertiesThatExcluded_ShouldNotPrint()
        {
            var str = ObjectPrinter.For<Person>()
                .Excluding(person => person.Name)
                .PrintToString(person);
            str.Should().NotContain("Name");
        }

        [Test]
        public void PrintToString_WithAlternativePrintingMethod_ShouldPrintPropertyCorrectly()
        {
            var str = ObjectPrinter.For<Person>()
                .Printing(person => person.Occupation)
                .Using(occupation => $"{occupation}!!!")
                .PrintToString(person);
            str.Should().Contain($"Occupation = Idle!!!");
        }

        [Test]
        public void PrintToString_WithAlternativePrintingMethodForPropertyType_ShoulPrintPropertiesCorrectly()
        {
            var str = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(num => (num - 10).ToString())
                .PrintToString(person);
            str.Should().ContainAll(new[] { "Height = 169,9", "Weight = 59,5" });
        }

        [TestCase("ru-RU")]
        [TestCase("en-US")]
        [TestCase("fr-FR")]
        public void PrintToString_ShouldPrintWithAlternativeCulture(string langCode)
        {
            var culture = new CultureInfo(langCode);
            var str = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(culture)
                .PrintToString(person);
            str.Should().ContainAll(new[]
            {
                $"Height = {person.Height.ToString(culture)}",
                $"Weight = {person.Weight.ToString(culture)}"
            });
        }

        [TestCase("Alexander", 4, "Ale")]
        [TestCase("Maksim", 4, "Maks")]
        [TestCase("Alex", 5, "Alex")]
        public void PrintToString_WithTrimmedToLength_ShouldPrintPropertyCorrectly(
            string name, int maxLength, string expected)
        {
            person.Name = name;
            var str = ObjectPrinter.For<Person>()
                .Printing(person => person.Name)
                .TrimmedToLength(maxLength)
                .PrintToString(person);
            str.Should().Contain($"Name = {expected}");
        }

        [Test]
        public void PrintToString_ShouldPrintArrayCorrectly()
        {
            var array = new string[] { "1", "2", "3", "4", "5" };
            var str = ObjectPrinter.For<string[]>().PrintToString(array);
            str.Should().Contain("[1, 2, 3, 4, 5]");
        }

        [Test]
        public void PrintToString_ShouldPrintListCorrectly()
        {
            var list = new List<int>() { 1, 2, 3, 4, 5 };
            var str = ObjectPrinter.For<List<int>>().PrintToString(list);
            str.Should().Contain("[1, 2, 3, 4, 5]");
        }

        [Test]
        public void PrintToString_ShouldPrintDictionaryCorrectly()
        {
            var dict = new Dictionary<int, string>
            {
                [1] = "one",
                [2] = "two",
                [3] = "three",
                [4] = "four",
                [5] = "five"
            };
            var str = ObjectPrinter.For<Dictionary<int, string>>().PrintToString(dict);
            str.Should().Contain("[[1] = one, [2] = two, [3] = three, [4] = four, [5] = five]");
        }

        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()

                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(num => $"{num - 10}")

                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)

                //4. Настроить сериализацию конкретного свойства
                .Printing(person => person.Name).Using(name => $"{name} Surname")

                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().TrimmedToLength(3)

                //6. Исключить из сериализации конкретного свойства
                .Excluding(person => person.Weight);

            var str = printer.PrintToString(person);
            Console.Write(str);
        }
    }
}