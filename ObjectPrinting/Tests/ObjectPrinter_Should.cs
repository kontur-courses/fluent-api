using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person person;

        [SetUp]
        public void SetUp() =>
            person = new Person
            {
                Name = "Alexander", Age = 42, ShoeSize = 42,
                Height = 4.2, Birthday = DateTime.MinValue, Id = new Guid()
            };
        
        [Test]
        public void Demo()
        {

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serializing<DateTime>().Using(x => x.Day.ToString() + '.' + x.Month.ToString())
                //3. Для числовых типов указать культуру
                .Serializing<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serializing(p => p.Id).Using(s => s.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializing(p => p.Name).TrimToLength(6)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.ShoeSize);
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            //var s2 = person.Serialize();
            //8. ...с конфигурированием
            //var s3 = person.Serialize(x=>x.Serializing(p=>p.Name).TrimToLength(2));
            printer.PrintToString(person).Should().Be("Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alexan\r\n\tAge = 42\r\n\tBirthday = 1.1\r\n");
        }

        [Test]
        public void ExcludeTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<double>();
            printer.PrintToString(person).Should().Be(" ");
        }
        
        
    }
}