using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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
            var arr = new Dictionary<string, string> {{"a", "a1"}, {"b", "b1"}, {"c", "c1"}};
            var arrPrinter = ObjectPrinter.For<IEnumerable>();

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<string>().Using(s => s.ToUpper())
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(s => new StringBuilder().Append(s).Append(" лет").ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).CutToLength(2)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            var num = 500;
            var numPrinter = ObjectPrinter.For<int>()
                .Printing<int>().Using(CultureInfo.InvariantCulture);
            var s0 = numPrinter.PrintToString(num);

            var s1 = printer.PrintToString(person);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию  
            var s2 = person.PrintToString();
            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding<Guid>());
        }
    }
}