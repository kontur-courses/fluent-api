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
        var person = new Person { Name = "Sergey", Surname = "Stakheev", Age = 22, Height = 184 };

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Excluding<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .Printing<int>().Using(number => number.ToString("X"))
            //3. Для числовых типов указать культуру
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .Printing(person => person.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
            .Excluding(person => person.Age);

        string s1 = printer.PrintToString(person);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        string s2 = person.PrintToString();

        //8. ...с конфигурированием
        string s3 = person.PrintToString(config => config.Excluding(person => person.Age));

        s1.Should().Contain("Name = Sergey").And.NotContain("Id =").And.NotContain("Age =");
        s2.Should().Contain("Id =").And.Contain("Age = 22");
        s3.Should().NotContain("Age =");
    }
}