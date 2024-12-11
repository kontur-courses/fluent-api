using System;
using System.Collections.Generic;
using System.Globalization;
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
            Name = "Alex",
            Age = 19,
            BestFriend = new Person
            {
                Name = "Bob",
                Age = 40,
                BestFriend = new Person()
            },
            Friends =
            [
                new Person
                {
                    Name = "Alice",
                    Age = 19,
                },
                new Person
                {
                    Name = "Robert",
                    Age = 19,
                },
            ],
            BodyParts =
            {
                { "Hand", 2 },
                { "Foot", 2 },
                { "Head", 1 },
                { "tail", 0 }
            }
        };

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Exclude<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .PrintSettings<int>().Using(i => i.ToString("X"))
            //3. Для числовых типов указать культуру
            .UseCulture<double>(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            .PrintSettings(x => x.Name).Using(p => $"-{p}-")
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .PrintSettings(p => p.Name).TrimmedToLength(2)
            //6. Исключить из сериализации конкретного свойства
            .Exclude(p => p.Age);
            
        var s1 = printer.PrintToString(person);
        Console.WriteLine(s1);
            
        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        var s2 = person.PrintToString();
        Console.WriteLine(s2);

        //8. ...с конфигурированием
        var s3 = person.PrintToString(p => p.Exclude(x => x.Name));
        Console.WriteLine(s3);
    }
}