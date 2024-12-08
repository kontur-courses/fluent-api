using System;
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

            var printer = ObjectPrinter.For<Person>();
                //1. Исключить из сериализации свойства определенного типа
                printer.ExceptType<string>();
                //2. Указать альтернативный способ сериализации для определенного типа
                printer.ForType<Person>()
                    .Use(o => o.ToString());
                //3. Для числовых типов указать культуру
                printer.ForType<double>()
                    .UseCulture(CultureInfo.InvariantCulture);
                //4. Настроить сериализацию конкретного свойства
                printer.ForProperty(x => x.Age) // TODO use Expression<Func<...>>
                    .Use(o => o.ToString());
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                printer.ForType<string>()
                    .UseMaxLength(15);
                //6. Исключить из сериализации конкретного свойства
                printer.ExceptProperty(nameof(Person.Name));
            
            string s1 = printer.PrintToString(person);
            // TODO use Verify.NUnit

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}