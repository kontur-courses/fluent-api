using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person = new Person();

        [SetUp]
        public void SetUp()
        {
            person.Age = 20;
            person.Name = "Ilya";
            person.Height = 170.5;
        }

        [Test]
        public void Excluding_ExcludesStringType()
        {
            //1. Исключить из сериализации свойства определенного типа

            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();
            printer.PrintToString(person).Split().Contains(nameof(person.Name)).Should().BeFalse();
        }

        [Test]
        public void Using_SetsAlternativePrintingFunction()
        {
            //2. Указать альтернативный способ сериализации для определенного типа

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => "XXX");
            var result = printer.PrintToString(person);
            result.Split().Contains("XXX").Should().BeTrue();
        }

        [Test]
        public void Using_SetsCulture_ToNumberTypes()
        {
            //3. Для числовых типов указать культуру
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture);
            var result = printer.PrintToString(person);
            result.Split().Contains(person.Height.ToString(CultureInfo.InvariantCulture)).Should().BeTrue();
        }

        [Test]
        public void Using_SetsAlternativePrinting_ToProperties()
        {
            //4. Настроить сериализацию конкретного свойства
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Height).Using(x => (x * 2).ToString());
            var result = printer.PrintToString(person);
            result.Split().Contains((person.Height * 2).ToString()).Should().BeTrue();
        }

        [Test]
        public void TrimmedToLength_Trims_StringPropertyValue()
        {
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            var length = 2;
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).TrimmedToLength(length);
            var result = printer.PrintToString(person);
            result.Split().Contains(person.Name.Substring(0, length)).Should().BeTrue();
        }

        [Test]
        public void Excluding_ExcludesProperties()
        {
            //6. Исключить из сериализации конкретное свойство
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Name);
            var result = printer.PrintToString(person);
            result.Split().Contains(person.Name).Should().BeFalse();
        }
    }
}