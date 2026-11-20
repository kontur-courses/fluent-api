using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Excluding_ShouldExcludeField_WhenFieldTypeMatches()
        {
            var person = new Person { Name = "Alex", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Excluding<float>();

            var actual = printer.PrintToString(person);

            actual.Should().NotContain("Score =").And.Contain("Age = 19");
        }

        [Test]
        public void Excluding_ShouldExcludeProperty_WhenPropertyTypeMatches()
        {
            var person = new Person { Name = "Alex", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            var actual = printer.PrintToString(person);

            actual.Should().NotContain("Age =").And.Contain("Score = 42");
        }

        [Test]
        public void Printing_ShouldUseCustomSerializer_WhenPropertyTypeMatches()
        {
            var person = new Person { Name = "Alex", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(age => age.ToString("X"));

            var actual = printer.PrintToString(person);

            actual.Should().Contain("Age = 13").And.Contain("Score = 42");
        }

        [Test]
        public void Printing_ShouldUseCustomSerializer_WhenFieldTypeMatches()
        {
            var person = new Person { Name = "Alex", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Printing<float>().Using(score => $"[{score}]");

            var actual = printer.PrintToString(person);

            actual.Should().Contain("Score = [42]").And.Contain("Age = 19");
        }

        [Test]
        public void Printing_ShouldUseCulture_WhenFormattingNumber()
        {
            var person = new Person { Name = "Alex", Height = 1.75 };
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture);

            var actual = printer.PrintToString(person);

            actual.Should().Contain("Height = 1.75");
        }

        [Test]
        public void Printing_ShouldUseCulture_WhenFormattingDate()
        {
            var date = new DateOnly(2024, 12, 31);
            var culture = CultureInfo.GetCultureInfo("fr-FR");
            var printer = ObjectPrinter.For<DateOnly>().Printing<DateOnly>().Using(culture);

            var actual = printer.PrintToString(date);

            actual.Should().Be("31/12/2024" + Environment.NewLine);
        }

        [Test]
        public void Printing_ShouldPreferProperty_WhenTypeFormatterIsAlsoConfigured()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(age => $"{age} years")
                .Printing<int>().Using(age => $"[{age}]");

            var actual = printer.PrintToString(person);

            actual.Should().Contain("Age = 19 years");
        }

        [Test]
        public void Printing_ShouldUseFieldCulture_WhenFieldIsSelected()
        {
            var person = new Person { Name = "Alex", Score = 42.5f };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Score).Using(CultureInfo.GetCultureInfo("ru-RU"));

            var actual = printer.PrintToString(person);

            actual.Should().Contain("Score = 42,5");
        }

        [TestCase("Alexander", 4, "Alex")]
        [TestCase("Alex", 10, "Alex")]
        [TestCase("Alex", 0, "")]
        public void TrimmedToLength_ShouldTrimmediString_WhenMaximumLengthIsConfigured(
            string name, int maxLen, string expectedName)
        {
            var person = new Person { Name = name };
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(maxLen);

            var actual = printer.PrintToString(person);

            actual.Should().Contain($"Name = {expectedName}{Environment.NewLine}");
        }

        [Test]
        public void Excluding_ShouldExcludeOnlySelectedProperty_WhenPropertiesHaveSameType()
        {
            var value = new { Age = 19, Score = 42 };

            var actual = value.PrintToString(config => config.Excluding(p => p.Age));

            actual.Should().NotContain("Age =").And.Contain("Score = 42");
        }

        [Test]
        public void Excluding_ShouldExcludeField_WhenFieldIsSelected()
        {
            var person = new Person { Name = "Alex", Age = 19, Score = 42 };
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Score);

            var actual = printer.PrintToString(person);

            actual.Should().NotContain("Score =").And.Contain("Age = 19");
        }

        [Test]
        public void PrintToString_ShouldFindCycle_WhenObjectsReferenceEachOther()
        {
            var first = new Person { Name = "first" };
            var second = new Person { Name = "second", Friend = first };
            first.Friend = second;
            var printer = ObjectPrinter.For<Person>();

            var actual = printer.PrintToString(first);
            
            Console.WriteLine(actual);
            actual.Should().Contain("Name = second").And.Contain("Friend = Parent");
        }

        [Test]
        public void PrintToString_ShouldPrintObjectTwice_WhenReferencedByDifferentMembers()
        {
            var child = new Person { Name = "shared" };
            var parent = new { Left = child, Right = child };

            var actual = parent.PrintToString();
            actual.Should().Contain("Left = Person").And.Contain("Right = Person").And.NotContain("Parent");
        }

        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}