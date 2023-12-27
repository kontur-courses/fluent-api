using System.Globalization;

namespace ObjectPrintingTests;

public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void Demo()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 165.31 };

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Exclude<Guid>()

            //2. Указать альтернативный способ сериализации для определенного типа
            .SetPrintingFor<int>().Using(prop => "Type printing")

            //3. Для числовых типов указать культуру
            .SetPrintingFor<double>().WithCulture(CultureInfo.CurrentCulture)

            //4. Настроить сериализацию конкретного свойства
            .SetPrintingFor(p => p.Name).Using(prop => "Property printing")

            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .SetPrintingFor(p => p.Name).TrimmedToLength(10)
            
            //5.1 Обрезание всех строковых свойств
            .SetPrintingFor<string>().TrimmedToLength(10)
            
            //6. Исключить из сериализации конкретного свойства
            .Exclude(p => p.Id);

        string s1 = printer.PrintToString(person);
        Console.WriteLine(s1);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        string s2 = person.PrintToString();
        Console.WriteLine(s2);

        //8. ...с конфигурированием
        string s3 = person.PrintToString(c => c.Exclude<int>());
        Console.WriteLine(s3);
    }
}