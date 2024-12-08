using System.Globalization;
using ObjectPrinting;

namespace ObjectPrintingTests;

public class Tests
{
    [Test]
    public void Demo()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 189.25 };

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Exclude<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .SetPrintingFor<int>().Using(_ => "Printing type")
            //3. Для числовых типов указать культуру
            .SetPrintingFor<double>().WithCulture(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            .SetPrintingFor(p => p.Name).Using(_ => "Property printing")
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .SetPrintingFor(p => p.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
            .Exclude(p => p.Id);
        var s1 = printer.PrintToString(person);
        Console.WriteLine(s1);
        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        var s2 = person.PrintToString();
        Console.WriteLine(s2);
        //8. ...с конфигурированием
        
    }
}