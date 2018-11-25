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
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(x => x.ToString())
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.CurrentCulture)
                //.Printing<string>().Using(CultureInfo.CurrentCulture); //нельзя
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(age => age.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().Trim(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Id);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию 
            var s2 = person.PrintToString();
            //8. ...с конфигурированием
            var s3 = person.PrintToString(config => config.Excluding(p => p.Id));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}