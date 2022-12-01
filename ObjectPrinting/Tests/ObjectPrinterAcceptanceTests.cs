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

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>().
                //2. Указать альтернативный способ сериализации для определенного типа
                Print<int>().As(x => x.ToString()).
                //3. Для числовых типов указать культуру
                Print<int>().As(CultureInfo.CurrentCulture).
                //4. Настроить сериализацию конкретного свойства
                Print(p => p.Name).As(n => $"#{n}").
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                Print(p => p.Name).Cut(5).
                //6. Исключить из сериализации конкретного свойства
                Excluding(p => p.Height);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}