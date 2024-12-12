using System;
using System.Globalization;
using System.Linq.Expressions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Serializer.Configs.Tools;
using ObjectPrinting.Tools;

namespace ObjectPrintingTests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void Demo()
    {
        var parent = new Person { Name = "Parent", Age = 20 };
        var person = new Person { Name = "Alex", Age = 19, Parent = parent };
        
        Expression<Func<Person, string>> test = p => p.Parent.Name;
        var b = test.TryGetPropertyName();
        Console.WriteLine(b);
        
        var type = typeof(Person);
        foreach (var propertyInfo in type.GetProperties())
        {
            Console.WriteLine($"{type.FullName}.{propertyInfo.Name}");
        }

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Excluding<Guid>()
            .Printing<string>().TrimmedToLength(10)
            //2. Указать альтернативный способ сериализации для определенного типа
            .Printing<int>().Using(i => i.ToString("X"))
            //3. Для числовых типов указать культуру
            .Printing<double>().WithCulture(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .Printing(p => p.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
            .Excluding(p => p.Age)
            .Excluding(p => p.Parent.Age);

        string s1 = printer.PrintToString(person);
            
        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        string s2 = person.PrintToString();
            
        //8. ...с конфигурированием
        string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
        Console.WriteLine(s1);
        Console.WriteLine(s2);
        Console.WriteLine(s3);
    }
}