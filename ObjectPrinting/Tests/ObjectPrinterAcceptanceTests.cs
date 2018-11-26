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
                .Exclude<T>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialize<T>().As(t => ToString)
                //3. Для числовых типов указать культуру
                .SetNumberCulture(CultureInfo).For<T>()
                //4. Настроить сериализацию конкретного свойства
                .Serialize(nameof(PropName)).As(p => ToString)
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serialize<string>().As(s => s.Substring(0, 3))
                //6. Исключить из сериализации конкретного свойства
                .Exclude(nameof(PropName));
            
            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}