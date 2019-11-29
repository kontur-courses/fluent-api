using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Core;
using ObjectPrinting.Core.PropertyPrintingConfig;
using ObjectPrinting.Infrastructure;
using ObjectPrintingTests.Infrastructure;

namespace ObjectPrintingTests.AcceptanceTests
{
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person(
                1f,"Alex", 179.9, 19, null);

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<float>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Friends);

            var s1 = printer.PrintToString(person);
            s1.Should().Be("Person\n\tName = Alex\r\n\tHeight = 179.9\r\n\tAge = 13\r\n");
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            s2.Should().Be("Person\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 179,9" +
                    "\r\n\tAge = 19\r\n\tFriends = null\r\n");

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            s3.Should().Be("Person\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 179,9" +
                           "\r\n\tFriends = null\r\n");
        }
    }
}