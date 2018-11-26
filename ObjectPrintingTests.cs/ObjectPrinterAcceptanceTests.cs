using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.SerializingConfig;

namespace ObjectPrintingTests.cs
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
                .Exclude<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialize<double>().Using(num => num.ToString(CultureInfo.InvariantCulture))
                //3. Для числовых типов указать культуру
                .Serialize<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serialize(p => p.Age).Using(p => p.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serialize<string>().Cut(2)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Height);

            var s1 = printer.PrintToString(person);
            Console.WriteLine($"{s1.GetType().FullName} = {s1}");

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            Console.WriteLine($"{s2.GetType().FullName} = {s2}");

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Exclude(p => p.Id));
            Console.WriteLine($"{s3.GetType().FullName} = {s3}");
        }
    }
}