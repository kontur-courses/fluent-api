using NUnit.Framework;
using System;
using System.Globalization;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person1 = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For(person1)
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<Guid>()

                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialize<DateTime>().Using(d => d.ToString())

                //3. Для числовых типов указать культуру
                .Serialize<byte>().Using(CultureInfo.CurrentCulture)
                .Serialize<short>().Using(CultureInfo.CurrentCulture)
                .Serialize<int>().Using(CultureInfo.CurrentCulture)
                .Serialize<long>().Using(CultureInfo.CurrentCulture)
                .Serialize<float>().Using(CultureInfo.CurrentCulture)
                .Serialize<double>().Using(CultureInfo.CurrentCulture)

                //4. Настроить сериализацию конкретного свойства
                .Serialize(p => p.Height).Using(a => a.ToString())

                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serialize(p => p.Name).WithMaxLength(10)

                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            string s1 = printer.PrintToString();

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            Person person2 = new Person { Name = "Bob", Age = 20 };
            string s2 = person2.GetObjectPrinter().PrintToString();

            //8. ...с конфигурированием
            Person person3 = new Person { Name = "Clara", Age = 21 };
            string s3 = person3
                .GetObjectPrinter()
                .Exclude<Guid>()
                .Exclude(p => p.Age)
                .PrintToString();
        }
    }
}
