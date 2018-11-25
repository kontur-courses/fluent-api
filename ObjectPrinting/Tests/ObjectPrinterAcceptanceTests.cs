using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using FluentAssertions;
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

            var printer = ObjectPrinter
                .For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serializing<Person>().Using(p => p.ToString())
                //3. Для числовых типов указать культуру
                .Serializing<int>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serializing(p => p.Age).Using(p => p.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializing(p => p.Name).SubstringValue(1, 3)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию 
            person
                .Serialize((cfg) => cfg
                    .Exclude(p => p.Age));
            //8. ...с конфигурированием
        }
    }
}