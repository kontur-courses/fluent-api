using System.Globalization;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public Task Demo()
    {
        var book = new Book
        {
            Title = "Преступление и наказание",
            Author = "Фёдор Достоевский",
            Description = "Роман о преступлении, нравственных терзаниях и искуплении.",
            PublishingHouse = "Русский вестник",
            PublishingYear = 1866
        };
        var printer = ObjectPrinter.For<Book>();
        //1. Исключить из сериализации свойства определенного типа
        //2. Указать альтернативный способ сериализации для определенного типа
        //3. Для числовых типов указать культуру
        //4. Настроить сериализацию конкретного свойства
        //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
        //6. Исключить из сериализации конкретного свойства

        printer
            .Exclude<int>()
            .SetTypeSerialization<string>(str => $"\"{str}\"")
            .SetNumericCulture(new CultureInfo("ru-RU"))
            .SetModifiedSerialization(
                b => b.PublishingHouse,
                s => $"Издательство: \"{s}\"")
            .Truncate(b => b.Description, 20)
            .Exclude(b => b.PublishingYear);
            
        var serialized = printer.PrintToString(book);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
        //8. ...с конфигурированием

        return ObjectPrinterTests.VerifySnapshot(serialized);
    }
}