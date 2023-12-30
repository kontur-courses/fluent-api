using System.Globalization;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrintingTests;

[TestFixture]
public class Acceptance
{
    [Test]
    public void Demo()
    {
        var person = new Person { Name = "Alex", Height = 180.5, Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .WithType<double>().SpecificSerialization(_ => "")
            .NumberCulture<double>(CultureInfo.CurrentCulture)
            .WithField(p => p.Age).SpecificSerialization(p => "")
            .TrimString(p => p.Name, 6)
            .WithField(p => p.Age).Exclude();

        //1. Исключить из сериализации свойства определенного типа
        //2. Указать альтернативный способ сериализации для определенного типа
        //3. Для числовых типов указать культуру
        //4. Настроить сериализацию конкретного свойства
        //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
        //6. Исключить из сериализации конкретного свойства

        var s1 = printer.PrintToString(person);
    }
}