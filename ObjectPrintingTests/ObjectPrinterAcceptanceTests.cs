using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Configs;
using ObjectPrintingTests;

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
                Name = "Alex",
                Surname = "Suvorov",
                Age = 19,
                Id = Guid.NewGuid(),
                Height = 3.14,
                Citizenship = "Russian"
            };

            var printer = ObjectPrinter.For<Person>()
                // 1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                // 2. Указать альтернативный способ сериализации для определенного типа
//                .Serializing<string>().Using(d => "Tom")
                // 3. Для числовых типов указать культуру
                .Serializing<double>().Using(CultureInfo.InvariantCulture)
                // 4. Настроить сериализацию конкретного свойства
                .Serializing(p => p.Age).Using(d => "100")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializing(p => p.Surname).Take(3)
                //6. Исключить из сериализации конкретноe свойство
                .Excluding(p => p.Citizenship);
            Console.WriteLine(printer.PrintToString(person));

//            var col = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 9, 8, 7, 6, 5, 4, 3, 2, 1};
//            var enumerablePrinter = ObjectPrinter.For<List<int>>();
//            var res = enumerablePrinter.PrintToString(col);
//            Console.WriteLine(res);

            var pr = ObjectPrinter.For<int>().Serializing<int>().Using(x=> "100");
            var i = 89;
            Console.WriteLine(pr.PrintToString(i));
        }
    }
}