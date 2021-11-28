using System;
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

        [SetUp]
        public void PersonCreator()
        {
            person = new Person
            {
                Name = "Alex",
                Age = 19,
                Height = 180,
                Id = Guid.Empty
            };
        }

        [Test]
        public void Demo()
        {
            person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .StringOf<int>(x => "re");
            printer.PrintToString(person);
            //1. Исключить из сериализации свойства определенного типа
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //6. Исключить из сериализации конкретного свойства

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [TestCase(typeof(Guid))]
        public void Without_Should_ExcludeExpectedTypes(Type type)
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>();

            printer.PrintToString(person).Should().Be("Person\n" +
                                                      "\tName = Alex\n" +
                                                      "\tHeight = 180\n" +
                                                      "\tAge = 19\n");
        }

        [Test]
        public void StringOf_Should_BeExpected()
        {
            var printer = ObjectPrinter.For<Person>()
                .StringOf<Person>(g => $"{g.Age}");
            
            var s= printer.PrintToString(person);
            printer.PrintToString(person).Should().Be("19");

        }

    }
}