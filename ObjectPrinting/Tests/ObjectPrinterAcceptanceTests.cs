using NUnit.Framework;
using System;
using System.Globalization;

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
                // DONE 1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. DONE Указать альтернативный способ сериализации для определенного типа
                .Serializing<int>().Using(x => x.ToString())
                //3. SCARCELY DONE Для числовых типов указать культуру
                .Serializing<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serializing(p => p.Name).Using(x => x.ToString())
                //5. SCARCELY DONE Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializing(p => p.Name).CutPrefix(5)
                //6. DONE Исключить из сериализации конкретного свойства
                .Excluding(p => p.Name);


            string s1 = printer.PrintToString(person);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            person.Serialize();
            //8. ...с конфигурированием
        }
    }
}