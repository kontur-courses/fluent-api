using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Extentions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectConfig.For<Person>()
                //1. Исключить из сериализации свойства определенного типа !
                .Exclude<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                //3. Для числовых типов указать культуру
                .ConfigureType<int>().SetCulture(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ConfigureProperty(p => p.Name).TrimByLength(2)
                //6. Исключить из сериализации конкретного свойства !
                .Exclude(p => p.Height)
                .ConfigurePrinter();

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            var s2 = person.PrintToString();
            Console.WriteLine(s2);
            
            //8. ...с конфигурированием
            var s3 = person.PrintToString(c => c.Exclude(p => p.Id).ConfigurePrinter());
            Console.WriteLine(s3);
        }
    }
}