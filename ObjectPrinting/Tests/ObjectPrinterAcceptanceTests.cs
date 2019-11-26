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
            var person = new Person {Name = "Alex", Age = 19, Height = 1213.123};

            var printer = ObjectPrinter.For<Person>()
                // 1. Исключить из сериализации свойства определенного типа 
                .Excluding<Guid>()
                // 2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => $"{i} years old")
                // . Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.GetCultureInfo("de-de"))
                // 4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Height).Using(d => $"{d} см.")
                // 5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(20)
                // 6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);

            // 7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            // 8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}