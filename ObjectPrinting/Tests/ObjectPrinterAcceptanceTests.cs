using System;
using System.Globalization;
using System.Security.Policy;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>()
                .Using(z => z.ToString())
                //3. Для числовых типов указать культуру
                .Printing<double>()
                .Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Name)
                .Using(z => z.ToString() + ":)")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name)
                .Trim(1)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Id);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию    
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString(z => z.Exclude(p => p.Id));

            Console.WriteLine(string.Join("\n", s1, s2, s3));
        }
    }
}