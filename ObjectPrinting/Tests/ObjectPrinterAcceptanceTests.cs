using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions.Common;
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
                // ✔ 1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                // ✔ 2. Указать альтернативный способ сериализации для определенного типа
                .For<string>().Use(s => s.Trim())
                // ✔ 3. Для числовых типов указать культуру
                .For<sbyte>().Use(CultureInfo.InvariantCulture)
                .For<byte>().Use(CultureInfo.InvariantCulture)
                .For<short>().Use(CultureInfo.InvariantCulture)
                .For<ushort>().Use(CultureInfo.InvariantCulture)
                .For<int>().Use(CultureInfo.InvariantCulture)
                .For<uint>().Use(CultureInfo.InvariantCulture)
                .For<long>().Use(CultureInfo.InvariantCulture)
                .For<float>().Use(CultureInfo.InvariantCulture)
                .For<double>().Use(CultureInfo.InvariantCulture)
                .For<decimal>().Use(CultureInfo.InvariantCulture)
                // ✔ 4. Настроить сериализацию конкретного свойства
                .For(p => p.Name).Use(s => s.Trim())
                // ✔ 5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .For(p => p.Name).Trim(4)
                .For<string>().Trim(4)
                // ✔ 6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);

            // ✔ 7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию     
            var s2 = person.PrintToString();
            // ✔ 8. ...с конфигурированием
            var s3 = person.Printing().Excluding<Guid>().PrintToString();
        }
    }
}