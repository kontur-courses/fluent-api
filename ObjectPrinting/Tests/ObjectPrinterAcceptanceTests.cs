using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Extensions;
using ObjectPrinting.Tests.TestClasses;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private readonly Person person = new()
        {
            Name = "Alex",
            Age = 19,
            Height = 70.5,
            Id = new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11),
            Surname = "Alexovich"
        };

        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(_ => "Age")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Height)
                //7. Уставноить максимальный уровень рекурсии
                .SetMaxNestingLevel(10);

            var s1 = printer.PrintToString(person);

            //8. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //9. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}