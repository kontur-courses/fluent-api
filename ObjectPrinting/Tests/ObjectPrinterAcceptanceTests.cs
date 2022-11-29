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
            var person = new Person { Name = "Alex", Age = 19, Height = 5.6, Width = 7.2f };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //6. Исключить из сериализации конкретного свойства
                .Excluding(x => x.Age)
                //4. Настроить сериализацию конкретного свойства
                .Printing(x => x.Name).Using(x => x.ToString() + "AAA")
                //3. Для числовых типов указать культуру
                .Printing<float>().Using(CultureInfo.InvariantCulture)
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<double>().Using(x => (x + 100).ToString());
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)

                var s1 = printer.PrintToString(person);
                //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
                //8. ...с конфигурированием
        }
    }
}