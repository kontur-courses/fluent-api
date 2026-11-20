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
            var person = new Person
            {
                Name = "Alexandr",
                Age = 19,
                Height = 182.5,
                Father = new Person { Name = "Ivan", Age = 45, Height = 178.3 }
            };

            var printer = ObjectPrinter.For<Person>()
                // 1. Исключить из сериализации свойства определенного типа
                .Exclude<Guid>()
                // 2. Указать альтернативный способ сериализации для определенного типа
                .For<int>().Using(i => i.ToString("X"))
                // 3. Для числовых типов указать культуру
                .For<double>().Using(CultureInfo.InvariantCulture)
                // 4. Настроить сериализацию конкретного свойства
                .For(p => p.Age).Using(age => $"{age} years")
                // 5. Настроить обрезание строковых свойств (метод виден только для string)
                .For(p => p.Name).Truncate(4)
                // 6. Исключить из сериализации конкретное свойство
                .Exclude(p => p.Father);
            
            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}