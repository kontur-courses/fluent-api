using NUnit.Framework;
using System.Globalization;

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
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialise<char>().Using((ch) => ((int)ch).ToString())
                //3. Для числовых типов указать культуру
                .Serialise<double>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serialise(p => p.Age).Using((ch) => (ch).ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serialise(p => p.Name).Trimming(3)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Id);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию 
            person.PrintToString();
            //8. ...с конфигурированием
            person.PrintToString((str) => str.Trim());
        }
    }
}