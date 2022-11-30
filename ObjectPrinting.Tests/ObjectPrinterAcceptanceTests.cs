using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialize<int>().Using(x => x.ToString())
                //3. Для числовых типов указать культуру
                .Serialize<double>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serialize(x => x.Age).Using(x=>x.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serialize(x=>x.Name).TrimmedToLength(4)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(x => x.Name);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию     
            var s2 = person.PrintToStringDefault();
            //8. ...с конфигурированием
            var s3 = person.PrintToString(x=>x.Excluding<string>());
        }
    }
}