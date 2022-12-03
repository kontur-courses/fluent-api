using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests;

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
            .ForType<Person>().Serialize(x => x.ToString())
            //3. Для числовых типов указать культуру
            .ForType<double>().Use(CultureInfo.CurrentCulture)
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .ForProperty(p => p.Name).TrimToLength(50)
            //6. Исключить из сериализации конкретного свойства
            .Excluding(x => x.Name);

        var s1 = printer.PrintToString(person);
        Console.WriteLine(s1);
        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию    
        var s2 = person.PrintToString();
        Console.WriteLine(s2);
        //8. ...с конфигурированием
        var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
        Console.WriteLine(s3);
    }

    private class Bar
    {
        public string Name { get; set; }
        public Bar Next { get; set; }
        public List<Bar> List { get; set; } = new();
        public Dictionary<int, Bar> Dict { get; set; } = new();
    }

    [Test]
    public void Demo2()
    {
        var a = new Bar { Name = "Bar1" };
        var b = new Bar { Name = "Bar2", Next = a };
        a.List.Add(b);
        a.List.Add(b);
        a.Dict.Add(0, b);
        a.Dict.Add(1, b);
        b.List.Add(a);
        b.List.Add(a);
        b.Dict.Add(0, a);
        b.Dict.Add(1, a);
        var printer = ObjectPrinter.For<Bar>();
        var actual = printer.PrintToString(a);
        Console.WriteLine(actual);
    }
}