using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var test = "";

            var printer = ObjectPrinter.For<Person>();

            string s1 = printer
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<string>().Using(s => s.ToUpper())
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p=>p.Name).Using(s=>s.ToUpper())
                //5.Настроить обрезание строковых свойств(метод должен быть виден только для строковых свойств)
                .Printing(p=>p.Name).TrimmedToLength(3)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Height)
                .PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            
            //8. ...с конфигурированием
        }

        [Test]
        public void PrintToString_WithoutProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var printer = ObjectPrinter.For<Person>();
            var s = printer.PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithExcludeType()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Excluding<Guid>()
                .PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithAlternativeSerializationType()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person\r\n\tId = Guid\r\n\tName = ALEX\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Printing<string>().Using(s => s.ToUpper())
                .PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithExcludeProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tAge = 19\r\n";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Excluding(p => p.Height)
                .PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithCultureInfo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.80};
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 1.8\r\n\tAge = 19\r\n";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Printing<double>().Using(new CultureInfo("en"))
                .PrintToString(person);
            s.Should().Be(expected);
        }

    }
}