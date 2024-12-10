using System.Globalization;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    public void Demo()
    {
        var person = new Person(new Guid("a1a2a3a4a5a6a7a8"), "Alex", 182, 19, DateTime.Today);

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Excluding<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .Printing<int>().Using(i => i.ToString("X"))
            //3. Для числовых типов указать культуру
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            .Printing<DateTime>().Using(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .Printing(p => p.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
            .Excluding(p => p.Age);

        var s1 = printer.PrintToString(person);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        var s2 = person.PrintToString();

        //8. ...с конфигурированием
        var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
    }
}