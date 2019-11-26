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
                .For<string>().Using(s => s.Trim())
                // ✔ 3. Для числовых типов указать культуру
                .For<sbyte>().Using(CultureInfo.InvariantCulture)
                .For<byte>().Using(CultureInfo.InvariantCulture)
                .For<short>().Using(CultureInfo.InvariantCulture)
                .For<ushort>().Using(CultureInfo.InvariantCulture)
                .For<int>().Using(CultureInfo.InvariantCulture)
                .For<uint>().Using(CultureInfo.InvariantCulture)
                .For<long>().Using(CultureInfo.InvariantCulture)
                .For<float>().Using(CultureInfo.InvariantCulture)
                .For<double>().Using(CultureInfo.InvariantCulture)
                .For<decimal>().Using(CultureInfo.InvariantCulture)
                // ✔ 4. Настроить сериализацию конкретного свойства
                .For(p => p.Name).Using(s => s.Trim())
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