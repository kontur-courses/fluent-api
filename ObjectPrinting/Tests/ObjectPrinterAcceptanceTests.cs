using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Common;
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
        private const string expectedFormat = "Person\r\n\tId = {0}\r\n\tName = {1}\r\n\tHeight = {2}";

        [Test]
        public void DemoWithPrinter()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = string.Format(expectedFormat, person.Id, person.Name.ToUpper().Substring(2), person.Height);

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .SerializeTypeAs<double>(number => "123: " + number.ToString())
                //3. Для числовых типов указать культуру
                .SetNumericTypeCulture<double>(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .ConfigurePropertySerialization(person => person.Name)
                    .SetSerializer(name => name.ToUpper())
                    //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                    .SetMaxLength(2)
                //6. Исключить из сериализации конкретного свойства
                .ExcludeProperty(person => person.Age);

            var actual = printer.PrintToString(person);
            actual.Should().Be(expected);
        }

        [Test]
        public void DemoWithObjectExtension()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = string.Format(expectedFormat, person.Id, person.Name.ToUpper().Substring(2), person.Height);

            var actual = person.PrintToString(config => config.Exclude<int>()
                                                              .SerializeTypeAs<double>(n => "123: " + n.ToString())
                                                              .ConfigurePropertySerialization(person => person.Name)
                                                                  .SetSerializer(name => name.ToUpper())
                                                                  .SetMaxLength(2)
                                                              .ExcludeProperty(person => person.Age));

            actual.Should().Be(expected);
        }
    }
}