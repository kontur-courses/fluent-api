using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private PrintingConfig<Person> printer;
        private Person person;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person
            {
                Name = "Albert",
                Age = 54,
                Height = 146,
                Id = new Guid()
            };
        }


        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
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

        [Test]
        public void Excluding_ShouldExcludePropertyOfSpecificType()
        {
            printer
                .Excluding<Guid>()
                .PrintToString(person)
                .Should()
                .Be("Person\r\n\tName = Albert\r\n\tHeight = 146\r\n\tAge = 54\r\n");
        }

        [Test]
        public void Printing_Using_ShouldReplaceSpecialTypeOutput()
        {
            printer
                .Printing<int>().Using(x => "")
                .PrintToString(person)
                .Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = Albert\r\n\tHeight = 146\r\n\tAge = \r\n");
        }
    }
}