using System;
using NUnit.Framework;
using ObjectPrinting.Solved.Extensions;
using ObjectPrinting.Solved.TestData;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            //var person = new Person { Name = "Alex", Age = 19 };
            var person = Person.GetInstance();

            var printer = ObjectPrinter.For<Person>()
                //1.Исключить из сериализации свойства определенного типа
                //.Excluding<Guid>()
                //2.Указать альтернативный способ сериализации для определенного типа
                //.PrintingType<int>().Using(i => i.ToString("X"))
                //3.Для числовых типов указать культуру
                //.PrintingType<double>().Using(CultureInfo.InvariantCulture)
                //4.Настроить сериализацию конкретного свойства
                .PrintingType<Guid>().Using(guid => "Abracadabra")
                .PrintingMember(p => p.Name).Using(name => name[0].ToString())
                //5.Настроить обрезание строковых свойств(метод должен быть виден только для строковых свойств)
                .PrintingType<string>().TrimmedToLength(3);
            //6.Исключить из сериализации конкретного свойства
            //.Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);
            //7.Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}