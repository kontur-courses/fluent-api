using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void UsageDemonstration()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 232.32432 };

            var objectPrinter =
                ObjectPrinter
                    .For<Person>()

                    // 1. Исключить из сериализации свойства определенного типа
                    .Excluding<Guid>()

                    // 2. Указать альтернативный способ сериализации для определенного типа
                    .Printing<int>().Using(integer => integer.ToString("X"))

                    // 3. Для числовых типов указать культуру
                    .Printing<double>().Using(CultureInfo.GetCultureInfo("ru"))

                    // 4. Настроить сериализацию конкретного свойства
                    .Printing(p => p.Name)

                    // 5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                    .TrimmedToLength(3)

                    // 6. Исключить из сериализации конкретного свойства
                    .Excluding(p => p.Age);

            string s1 = objectPrinter.PrintToString(person);

            // 7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            // 8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));

            TestContext.WriteLine(s1);
            TestContext.WriteLine(s2);
            TestContext.WriteLine(s3);
        }
    }
}