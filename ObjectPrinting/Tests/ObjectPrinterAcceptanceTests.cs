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

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()

                //2. Указать альтернативный способ сериализации для определенного типа
                .Serializing<DateTime>().Using(d => d.ToString())

                //3. Для числовых типов указать культуру
                .Serializing<byte>().Using(CultureInfo.CurrentCulture)
                .Serializing<short>().Using(CultureInfo.CurrentCulture)
                .Serializing<int>().Using(CultureInfo.CurrentCulture)
                .Serializing<long>().Using(CultureInfo.CurrentCulture)
                .Serializing<float>().Using(CultureInfo.CurrentCulture)
                .Serializing<double>().Using(CultureInfo.CurrentCulture)

                //4. Настроить сериализацию конкретного свойства
                .Serializing(p => p.Name).Using(a => a.ToString())

                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializing<string>(p => p.Name).WithMaxLength(10)
                .Serializing<string>().WithMaxLength(10)

                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Height);

            string s1 = printer.PrintToString(person1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            Person person2 = new Person { Name = "Bob", Age = 20 };
            string s2 = person2.PrintToString();

            //8. ...с конфигурированием
            Person person3 = new Person { Name = "Clara", Age = 21 };
            string s3 = person3
                .GetObjectPrinter()
                .Excluding<Guid>()
                .Excluding(p => p.Age)
                .PrintToString();
        }
    }
}