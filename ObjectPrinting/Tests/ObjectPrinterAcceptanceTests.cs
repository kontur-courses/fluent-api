using System.Globalization;
using ObjectPrinting.Tests.DTOs;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterAcceptanceTests
    {
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ExcludeType<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .SerializeType<string>(x => x.ToUpper())
                //3. Для числовых типов указать культуру
                .SetCulture<decimal>(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .ConfigureProperty(p => p.Name).SetSerialization(x => x + "22")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ConfigureProperty(p => p.Name).Crop(5)
                ////6. Исключить из сериализации конкретного свойства
                .ExcludeProperty(p => p.Name);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string defaultPrinting = s1.PrintToString();
            //8. ...с конфигурированием
            string configuredPrinting = s1.PrintingWithConfigure(config => new PrintingConfig<string>());
        }
    }
}