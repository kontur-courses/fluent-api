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
            var person = new Person { Name = "Alex", Age = 19 };

            var test = typeof(Person);
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2.Указать альтернативный способ сериализации для определенного типа
                .ChangePrintFor<int>().Using((i => (i + 100).ToString()))
                ////3. Для числовых типов указать культуру
                //.ChangePrintFor<int>().Using(CultureInfo.CurrentCulture);
                ////4. Настроить сериализацию конкретного свойства
                .ChangePrintFor(p => p.Height).Using(value => value.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ChangePrintFor(p => p.Name).TrimToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Height);

            string s1 = printer.PrintToString(person);
            Console.Write(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию    
            //8. ...с конфигурированием
        }
    }
}