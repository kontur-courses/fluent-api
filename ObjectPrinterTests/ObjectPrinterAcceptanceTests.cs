using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinterTests.TestClasses;
using ObjectPrinting;
using ObjectPrinting.Config.Member;
using ObjectPrinting.Config.Type;

namespace ObjectPrinterTests
{
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person(Guid.NewGuid().ToString());
        }

        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()

                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))

                //3. Для числовых типов указать культуру
                .Printing<DateTime>().Using(CultureInfo.CurrentCulture)

                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Height).Using(i => "hh: " + i.ToString())

                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)

                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age1)
                
                // 7. Настроить максимальный уровень вложенности
                .SetMaxNestingLevel(1);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age1));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}
