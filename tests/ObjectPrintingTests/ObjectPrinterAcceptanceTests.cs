﻿using System.Globalization;
using ObjectPrinting;

namespace ObjectPrintingTests
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
                .For<int>().Using(i => i.ToString())
                //3. Для числовых типов указать культуру
                .For<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .For(p => p.Age).Using(age => age.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .For(p => p.Name).MaxLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(x => x.Name);
            
            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            //8. ...с конфигурированием
            var s3 = person.PrintToString(config => config);
        }
    }
}