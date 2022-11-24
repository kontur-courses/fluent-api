using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
                .SerializeTypeAs<int>(number => "123: " + number.ToString())
                //3. Для числовых типов указать культуру
                .SetNumericTypeCulture<double>(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .ConfigurePropertySerialization(person => person.Name)
                    .SetSerializer(name => name.ToUpper())
                    //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                    .SetMaxLength(10)
                //6. Исключить из сериализации конкретного свойства
                .ExcludeProperty(person => person.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            //person.Serialize();
            //8. ...с конфигурированием
        }


        private void DemoVersion()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .SerializeTypeAs<int>(number => "123: " + number.ToString())
                //3. Для числовых типов указать культуру
                .SetNumericTypeCulture<double>(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .ConfigurePropertySerialization(person => person.Name)
                    .SetSerializer(name => name.ToUpper())
                 //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                    .SetMaxLength(10)
                //6. Исключить из сериализации конкретного свойства
                .ExcludeProperty(person => person.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            //person.Serialize();
            //8. ...с конфигурированием
        }
    }
}