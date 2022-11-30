using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;
using ObjectPrintingShould.ObjectsForTest;

namespace ObjectPrintingShould;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void Demo()
    {
        var person = new Person {Name = "Alex", Age = 19};
        var collections = new Collections
        {
            Arr = new[] {1, 2, 3}, Lis = new List<string> {"dafs", "fkda;", "fdjai"},
            Dict = new Dictionary<int, int> {{1, 1}, {2, 2}, {3, 3}}
        };
        var printerCol = ObjectConfig.For<Collections>().Build();
        
        var printer = ObjectConfig.For<Person>()
            //1. Исключить из сериализации свойства определенного типа !
            .Exclude<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            .ConfigureType<int>().SetCulture(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .ConfigureProperty(p => p.Name).TrimByLength(2)
            //6. Исключить из сериализации конкретного свойства !
            .Exclude(p => p.Height)
            .Build();
        
        var s = printerCol.PrintToString(collections);
        Console.WriteLine(s);
        
        // var s1 = printer.PrintToString(person);
        // Console.WriteLine(s1);
        //
        // //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
        // var s2 = person.PrintToString();
        // Console.WriteLine(s2);
        //
        // //8. ...с конфигурированием
        // var s3 = person.PrintToString(c => c.Exclude(p => p.Id).Build());
        // Console.WriteLine(s3);
        //
        // var persons = new Person[] {new() {Name = "Alex"}, new() {Name = "Vova"}, new() {Name = "Kirill"}};
        // var printerPersons = ObjectConfig.For<Person[]>().Build();
        // var i = printerPersons.PrintToString(persons);
        // var g = 1;
        //
        // var item = ObjectConfig
        //     .For<Order>()
        //     .ConfigureType<Item>(i => i.Name)
        //     .Build()
        //     .PrintToString(new Order { Item = new Item { Name = "some item" }, Quantity = 1000 });
        //
        // Console.WriteLine(item);
    }

    [Test]
    public void PrintToString_ArrayOfPerson_SerializeArray()
    {
        var persons = new Person[] {new() {Name = "Alex"}, new() {Name = "Vova"}, new() {Name = "Kirill"}};
        ObjectConfig.For<Person[]>()
            .Build()
            .PrintToString(persons)
            .Should()
            .Be("[\nPerson\n\tId = 00000000-0000-0000-0000-000000000000\n\tName = Alex\n\tHeight = 0\n\tAge = 0\n\n" +
                "Person\n\tId = 00000000-0000-0000-0000-000000000000\n\tName = Vova\n\tHeight = 0\n\tAge = 0\n\n" +
                "Person\n\tId = 00000000-0000-0000-0000-000000000000\n\tName = Kirill\n\tHeight = 0\n\tAge = 0\n]");
    }

    [Test]
    public void PrintToString_PersonWithNameTrim_PersonWithTrimName()
    {
        ObjectConfig
            .For<Person>()
            .ConfigureProperty(p => p.Name)
            .TrimByLength(4)
            .Exclude<Guid>()
            .Build()
            .PrintToString(new Person {Name = "Sasha"})
            .Should()
            .Be("Person\n\tName = Sash\n\tHeight = 0\n\tAge = 0\n");
    }

    [Test]
    public void PrintToString_PersonWithExcludeType_StringWithoutExclude()
    {
        ObjectConfig
            .For<Person>()
            .Exclude<Guid>()
            .Build()
            .PrintToString(new Person {Name = "Sasha"})
            .Should()
            .Be("Person\n\tName = Sasha\n\tHeight = 0\n\tAge = 0\n");
    }

    [Test]
    public void PrintToString_PersonWithExcludeProperty_StringWithoutProperty()
    {
        ObjectConfig
            .For<Person>()
            .Exclude(p => p.Id)
            .Build()
            .PrintToString(new Person {Name = "Sasha"})
            .Should()
            .Be("Person\n\tName = Sasha\n\tHeight = 0\n\tAge = 0\n");
    }

    [Test]
    public void PrintToString_NestedClasses_SerializeString()
    {
        ObjectConfig
            .For<Order>()
            .ConfigureType<Item>(i => i.Name)
            .Build()
            .PrintToString(new Order {Item = new Item {Name = "some item"}, Quantity = 1000})
            .Should()
            .Be("Order\n\tItem = some item\n\tQuantity = 1000\n");
    }

    [Test]
    public void PrintToString_ItemExclude_ItemWithSerialize()
    {
        var person = new Person {Name = "Alex"};
        var printer = ObjectConfig.For<Person>().Exclude(p => p.Id).Exclude(p => p.Height).Exclude(p => p.Age).Build();
        printer.PrintToString(person).Should().Be("Person\n\tName = Alex\n");
    }

    [Test]
    public void PrintToString_Null_StringWithNullMessage()
    {
        ObjectConfig.For<Person>().Build().PrintToString(null!).Should().Be("null\n");
    }
}