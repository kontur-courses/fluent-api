using System.Globalization;
using ObjectPrinting;
using ObjectPrinting.Configs.Extensions;
using ObjectPrintingTests.Models;

namespace ObjectPrintingTests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void Demo()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 178.2, Id = Guid.NewGuid() };

        var printer = ObjectPrinter
            .InClass<Person>()

            //1. Исключить из сериализации свойства определенного типа
            .For<Guid>().Exclude()

            //2. Указать альтернативный способ сериализации для определенного типа
            .For<double>().Use(d => $"{d:F1} cm")

            //3. Для числовых типов указать культуру
            .For<double>().Use(CultureInfo.InvariantCulture)

            //4. Настроить сериализацию конкретного свойства
            .For(p => p.Name).Use(name => $"Current name is {name}")

            //5. Настроить обрезание строковых свойств
            .For<string>().Trim(4)
            .For(p => p.Name).Trim(3)

            //6. Исключить конкретное свойство
            .For(p => p.Age).Exclude();

        var s1 = printer.PrintToString(person);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        var s2 = person.PrintToString();
        //8. ...с конфигурированием
        var s3 = person.PrintToString(config => config
            .For<Guid>().Exclude()
            .For(p => p.Name).Trim(2)
        );
    }
}