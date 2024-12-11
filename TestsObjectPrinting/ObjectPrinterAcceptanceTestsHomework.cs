using System.Globalization;
using ObjectPrintingHomework;

namespace TestsObjectPrinting;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void Demo()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Excluding<int>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .Printing<int>().Using(i => i.ToString("X"))
            //3. Для числовых типов указать культуру
            .Printing<double>().Using(i => new CultureInfo("ar-EG"))
            //4. Настроить сериализацию конкретного свойства
            .Printing<double>(p => p.Height).Using(i => new CultureInfo("ar-EG"))
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .Printing(p => p.Name).TrimmedToLength(0, 10)
            //6. Исключить из сериализации конкретного свойства
            .Excluding(p => p.Age);

        string s1 = printer.PrintToString(person);
        Console.WriteLine(s1);
    }
}