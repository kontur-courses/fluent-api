using System;
using System.Collections.Generic;
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
            var configuration = ObjectPrinter.For<Person>();

            //1. Исключить из сериализации свойства определенного типа
            configuration.Excluding<Guid>();
            //2. Указать альтернативный способ сериализации для определенного типа
            configuration.For<string>().WithSerialization(p => p.ToString() + "changed");

            configuration.For<int>().WithSerialization(x => (x + 1).ToString("0.##") + "Changed").For<int>().WithCulture(CultureInfo.InvariantCulture);

            //3. Для числовых типов указать культуру
            configuration.For<int>().WithCulture(CultureInfo.CurrentCulture);
            //4. Настроить сериализацию конкретного свойства
            configuration.For(p => p.Age).WithSerialization(p => p.ToString());
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            configuration.For(p => p.Name).Trim(4);
            // printer.For(p => p.Age).Trim(4);
            //6. Исключить из сериализации конкретного свойства
            configuration.Excluding(p => p.Name);
            configuration.For(p => p.Age).WithSerialization(p => p.ToString());

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию  
            person.PrintToString();
            //8. ...с конфигурированием
            person.PrintToString(settings => settings.For<string>().WithSerialization(p => p.ToString() + "changed"));

            ObjectPrinter.PrintToString(person, configuration);
        }
    }
}