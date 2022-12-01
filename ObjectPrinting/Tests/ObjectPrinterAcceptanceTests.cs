using NUnit.Framework;
using System;
using System.Globalization;
using ObjectPrinting;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Surname = "Foster", Name = "Alex", Age = 19, Height = 180, Weight = 83.65 };

            var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Excluding<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .Printing<string>().Using(i => i.ToUpper())
            //3. Для числовых типов указать культуру
            .Printing<double>().Using(new CultureInfo("en"))
            //4. Настроить сериализацию конкретного свойства
            .Printing(p => p.Age).Using(age => $"{age} years old")
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .Printing(p => p.Name).TrimmedToLength(2)
            //6. Исключить из сериализации конкретного свойства
            .Excluding(p => p.Height);

            string s1 = printer.PrintToString(person);

            //7.Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
        }
    }
}