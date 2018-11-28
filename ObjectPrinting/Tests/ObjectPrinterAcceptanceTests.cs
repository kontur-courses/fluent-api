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
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialize<string>().As(t => t.ToString() + " - string")
                //3. Для числовых типов указать культуру
                .SetCulture(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serialize(p => p.Name).As(p => "Name: " + p.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serialize<string>().As(s => s.Substring(0, 3))
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Id);
            
            string s1 = printer.PrintToString(person);
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию   
            string s2 = person.PrintToStr();
            //8. ...с конфигурированием
            string s3 = person.PrintToStr(o => o.Exclude<int>());
        }
    }
}