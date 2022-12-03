using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Configuration;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person
            {
                Name = "max",
                Age = 19
            };

            var printer = new ObjectPrinter<Person>();
            printer.Configurate()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(
                    i => i.ToString("X"))
                //3. Для числовых типов указать культуру 
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Id).Using(guid => $"this is guid {guid}")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            
            //8.с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            var s4 = new ObjectPrinter<Person>(s => s.Excluding<int>()).PrintToString(person);
        }
    }
}