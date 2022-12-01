using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        //[Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //+1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //+2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //+3. Для всех типов, имеющих культуру, есть возможность ее указать
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing<DateTime>().Using(CultureInfo.CurrentUICulture)
                //+4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Height).Using(h => $"{h} meters")
                //+5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //+6. Исключить из сериализации конкретного свойства/поля
                .Excluding(p => p.Age);
                //7. Корректная обработка циклических ссылок между объектами (не должны приводить к StackOverflowException)

            string s1 = printer.PrintToString(person);
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();
            
            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}