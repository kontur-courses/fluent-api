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
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .AlternativeFor<int>().Using((p) => p.ToString())
                //3. Для числовых типов указать культуру
                .AlternativeFor<double>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .AlternativeFor(p => p.Name).Using(p => p.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .AlternativeFor(p => p.Name).TakeOnly(20)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Name);

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