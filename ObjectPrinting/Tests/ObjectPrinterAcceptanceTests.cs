using NUnit.Framework;
using System;
using System.Globalization;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void Demo()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Exclude<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .Printing<string>().Using(s => $"Строка - {s}")
            //3. Для числовых типов указать культуру
            .Printing<int>().WithCulture(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            .Printing(p => p.Age).Using(a => $"Мне сегодня {a} лет")
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .Printing<string>().Cut(2)
            //6. Исключить из сериализации конкретного свойства
            .Exclude(p => p.Id);
        
        var s1 = printer.PrintToString(person);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        var s2 = person.PrintToString();
        //8. ...с конфигурированием
        var s3 = person.PrintToString(c => c.Exclude<Guid>());
    }
}