using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;
using ObjectPrintingTests.ClassesForTests;

namespace ObjectPrintingTests
{
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person(Guid.Empty, "Alex", "Johnson", 180.5, 19);
            var serializingDictionary = person.GetDefaultPersonSerializingDictionary();

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age)
                .SetNestingLevel(3);

            var s1 = printer.PrintToString(person);
            var expected1 = TestHelper.GetExpectedResult(typeof(Person), new Dictionary<string, string>
            {
                {nameof(person.Name), person.Name},
                {nameof(person.Surname), person.Surname},
                {nameof(person.Height), person.Height.ToString(CultureInfo.InvariantCulture)}
            });
            s1.Should().BeEquivalentTo(expected1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            var expected2 =
                TestHelper.GetExpectedResult(typeof(Person), serializingDictionary);
            s2.Should().BeEquivalentTo(expected2);

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            serializingDictionary.Remove(nameof(person.Age));
            var expected3 = TestHelper.GetExpectedResult(typeof(Person), serializingDictionary);
            s3.Should().BeEquivalentTo(expected3);

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}