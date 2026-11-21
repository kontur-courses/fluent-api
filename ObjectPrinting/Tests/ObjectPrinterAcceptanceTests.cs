using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void Demo()
    {
        var person = new Person
        {
            Name = "Alexandr",
            Age = 19,
            Height = 182.5,
            Father = new Person { Name = "Ivan", Age = 45, Height = 178.3 }
        };

        var printer = ObjectPrinter.For<Person>()
            // 1. Исключить из сериализации свойства определенного типа
            .Exclude<Guid>()
            // 2. Указать альтернативный способ сериализации для определенного типа
            .For<int>().Using(i => i.ToString("X"))
            // 3. Для числовых типов указать культуру
            .For<double>().Using(CultureInfo.InvariantCulture)
            // 4. Настроить сериализацию конкретного свойства
            .For(p => p.Age).Using(age => $"{age} years")
            // 5. Настроить обрезание строковых свойств (метод виден только для string)
            .For(p => p.Name).Truncate(4)
            // 6. Исключить из сериализации конкретное свойство
            .Exclude(p => p.Father);

        string s1 = printer.PrintToString(person);

        // 7. Синтаксический сахар в виде метода расширения, сериализующего по умолчанию
        string s2 = person.PrintToString();

        // 8. ...с конфигурированием
        string s3 = person.PrintToString(cfg => cfg
            .Exclude<Guid>()
            .For(p => p.Name).Truncate(4));

        Console.WriteLine(s1);
        Console.WriteLine(s2);
        Console.WriteLine(s3);

        s1.Should()
            .NotContain("Id =")
            .And.NotContain("Father =")
            .And.Contain("Name = Alex" + Environment.NewLine)
            .And.Contain("Height = 182.5" + Environment.NewLine)
            .And.Contain("Age = 19 years" + Environment.NewLine);

        s2.Should()
            .Contain("Id = ")
            .And.Contain("Name = Alexandr" + Environment.NewLine)
            .And.Contain("Father = Person" + Environment.NewLine);

        s3.Should()
            .NotContain("Id =")
            .And.Contain("Name = Alex" + Environment.NewLine);
    }
}
