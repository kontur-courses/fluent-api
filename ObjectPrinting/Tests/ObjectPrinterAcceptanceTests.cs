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
                .Exclude<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialize<double>().Using(num => num.ToString())
                //3. Для числовых типов указать культуру
                .Serialize<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serialize(p => p.Age).Using(p => p.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serialize<string>().Cut(5)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Id);
                
            var s1 = printer.PrintToString(person);
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию 
            var s2 = person.PrintToString();
            
            //8. ...с конфигурированием
            var s3 = person.PrintToString(config => config.Exclude(p => p.Id));
        }
    }
}