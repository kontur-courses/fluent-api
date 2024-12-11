using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 180, Id = new Guid() };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialize<string>().With(str => str.ToUpper())
                //3. Для числовых типов указать культуру
                .SetCultureFor<double>(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serialize<string>(p => p.Name).With(name => name.ToUpper())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .TrimString(p => p.Name, 10)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age)
                ;

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
            Assert.Pass();
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            person.PrintToString();
            //8. ...с конфигурированием
            person.PrintToString(c => c.Exclude(p => p.Age));
        }
    }
}