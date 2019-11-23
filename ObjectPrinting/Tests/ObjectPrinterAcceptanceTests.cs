using System;
using System.Runtime.Serialization;
using NUnit.Framework;
using ObjectPrinting.Extensions;

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
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialize<string>().Using(s => s.Trim())
                //3. Для числовых типов указать культуру
                .Serialize<int>().Using(System.Globalization.CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serialize(p => p.Name).Using(str => "Name: " + str)
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serialize(p => p.Name).Take(5)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);
            
            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            person.Serialized();
            //8. ...с конфигурированием
            person.Serialized().Excluding(p => p.Age);
        }
    }
}