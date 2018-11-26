using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        [SetUp]
        public void Init()
        {
            person = new Person(Guid.NewGuid(), "Alex",192.57,21,15);
        }
        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serializer<double>().Using(i => i.ToString())
                //3. Для числовых типов указать культуру
                .Serializer<int>().SetCultureInfo(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serializer(p => p.GetType().GetProperty("Age").Name).Using(i => i.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializer(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Exclude("Age");

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void ObjectPrinter_Should_ExcludePropertyByType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<int>()
                .Exclude<Guid>()
                .Exclude<float>()
                .Exclude<string>();
            var result = printer.PrintToString(person);
            var expectedResult = "Person\r\n\tHeight = 192,57\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }

        [Test]
        public void ObjectPrinter_Should_ExcludePropertyByName()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude("Height")
                .Exclude("ArmLength")
                .Exclude("Id");
            var result = printer.PrintToString(person);
            var expectedResult = "Person\r\n\tName = Alex\r\n\tAge = 21\r\n";

            result.Should().BeEquivalentTo(expectedResult);
            Console.WriteLine(result);
        }
    }
}