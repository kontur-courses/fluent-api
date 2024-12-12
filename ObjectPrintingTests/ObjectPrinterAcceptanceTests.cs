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
        var parent2 = new Person { Name = "Parent2", Age = 21 };
        var parent = new Person { Name = "Parent", Age = 20, Parent = parent2};
        var person = new Person { Name = "Alex", Age = 19, Parent = parent };

        var printer = ObjectPrinter.For<Person>()
            .WithMaxNestingLevel(2)
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
    }
}