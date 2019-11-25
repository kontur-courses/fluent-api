using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа v
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа v
                .Serializing<int>().Using(t => t.ToString())
                //3. Для числовых типов указать культуру v
                .Serializing<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства v
                .Serializing(p => p.Age).Using(t => t.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств) v
                .Serializing(p => p.Name).Using(s => s.Trim())
                //6. Исключить из сериализации конкретного свойства v
                .Excluding(p => p.Id);
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}