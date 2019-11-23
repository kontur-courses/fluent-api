using System;
using System.Collections;
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

            Func<int, string> d = null;
            Func<string, string> s = null;
            CultureInfo loc = null;
            Func<string, string> a = null;

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Queue>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(x => "abcd")
                //3. Для числовых типов указать культуру
                .Printing<int>().SetCulture(loc)
                //4. Настроить сериализацию конкретного свойства
                .Printing(x => x.Name).Using(x => "Name")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimToLength(5)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(x => x.Name);
            
            string s1 = printer.PrintToString(person);

            string a2 = "5";
            a2.Serialize().Printing<int>().Using(x => "abcd");
            var conf = (a2.Serialize() as IPropertyPrintingConfig<int>)?.Config;
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}