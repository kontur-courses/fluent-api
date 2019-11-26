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
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .ForProperty<string>()
                    .SetFormat(s => s.Substring(0, 3))
                //3. Для числовых типов указать культуру
                .ForProperty<int>()
                    .SetFormat(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .ForProperty(x => x.Age)
                    .SetFormat(x => $"Моё форматирование) {x}")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ForProperty(x => x.Name)
                    .Cut(5)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(x => x.Height);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}