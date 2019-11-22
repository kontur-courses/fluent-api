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
                .Excluding<Guid>()
                .Printing<string>().Using(s => s.Trim())
                .Printing<int>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name).Using(s => s.Trim())
                .Printing(p => p.Name).Substring(0, 15)  
                .Printing<string>().Substring(3, 10)
                .Excluding(p => p.Age);

            //✔1. Исключить из сериализации свойства определенного типа
            //✔2. Указать альтернативный способ сериализации для определенного типа
            //✔3. Для числовых типов указать культуру
            //✔4. Настроить сериализацию конкретного свойства
            //✔5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //✔6. Исключить из сериализации конкретного свойства

            var s1 = printer.PrintToString(person);

            person.PrintToString();
            var s2 = person.Printing().Excluding<Guid>().PrintToString(35);
            //✔7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //✔8. ...с конфигурированием
        }
    }
}