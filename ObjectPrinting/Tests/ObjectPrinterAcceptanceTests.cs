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

            var printer = ObjectPrinter.For<Person>();

            //1. Исключить из сериализации свойства определенного типа
            printer.Excluding<Guid>();
            //2. Указать альтернативный способ сериализации для определенного типа
            printer.For<string>().WithSerialization(p => p.ToString() + "changed");
            //3. Для числовых типов указать культуру
            printer.For<int>().WithCulture(CultureInfo.CurrentCulture);
            //4. Настроить сериализацию конкретного свойства
            printer.For(p => p.Age).WithSerialization(p => p.ToString());
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            printer.For(p => p.Name).Trim(4);
            // printer.For(p => p.Age).Trim(4);
            //6. Исключить из сериализации конкретного свойства
            printer.Excluding(p => p.Name);
            printer.For(p => p.Age).WithSerialization(p => p.ToString());

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию  
            printer.PrintToString(person);
            person.PrintToString();
            //8. ...с конфигурированием
            person.PrintToString(settings => settings.For<string>().WithSerialization(p => p.ToString() + "changed"));

        }
    }
}