using System;
using System.Collections.Generic;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    private Person person;
    
    [SetUp]
    public void Setup() => person = new Person { Name = "Alex", Age = 19, Height = 1.81 };
    
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void Demo()
    {
        person.DateOfBirth = new DateTime(1985, 9, 9);
        var printer = ObjectPrinter.For<Person>();
        var s = printer.PrintToString(person);

        Approvals.Verify(s);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WhenExcludingTypeFromSerialization()
    {
        person.DateOfBirth = new DateTime(2004, 9, 9);
        var printer = ObjectPrinter
            .For<Person>()
            .Excluding<DateTime>();   //1. Исключить из сериализации свойства определенного типа
        
        var s = printer.PrintToString(person);

        Approvals.Verify(s);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WithCustomSerializationForSpecificType()
    {
        person.DateOfBirth = new DateTime(2004, 9, 9);
        var printer = ObjectPrinter
            .For<Person>()
            .Printing<DateTime>().Using(i => i.ToLongDateString());   
        //2. Указать альтернативный способ сериализации для определенного типа
        
        var s = printer.PrintToString(person);
        
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WhenSerializingWithCustomCulture()
    {
        var printer = ObjectPrinter
            .For<Person>()
            .Printing<double>().Using(CultureInfo.InvariantCulture); 
        //3. Для числовых типов указать культуру
        
        var s = printer.PrintToString(person);
        
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WhenTrimmingStringProperties()
    {
        var printer = ObjectPrinter
            .For<Person>()
            .Printing(p => p.Name)   //4. Настроить сериализацию конкретного свойства
            .TrimmedToLength(2);
        //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
        
        var s = printer.PrintToString(person);

        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WhenExcludingSpecificProperty()
    {
        var printer = ObjectPrinter
            .For<Person>()
            .Excluding(p => p.Height);  
        //6. Исключение из сериализации конкретного свойства/поля
        
        var s = printer.PrintToString(person);

        Approvals.Verify(s);
    }
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WhenChainingSerializationRules()
    {
        person.Name = "Maxwell";
        var printer = ObjectPrinter
            .For<Person>()
            .Excluding<Guid>()
            .Printing<DateTime>().Using(i => i.ToLongDateString())
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            .Printing(p => p.Name).TrimmedToLength(5)
            .Excluding(p => p.Age);
        
        var s = printer.PrintToString(person);

        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WhenUsingExtensionMethod()
    {
        var s = person.PrintToString();  
        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию

        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WithConfiguration()
    {
        var s = person.PrintToString(s => s.Excluding(p => p.Age));  
        //8. ...с конфигурированием

        Approvals.Verify(s);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldDetectCyclicReferences()
    {
        var father = new Person
        {
            Name = "Pavel Doe", 
            Age = 68, 
            DateOfBirth = new DateTime(1954, 9, 9),
            Father = person
        };
        person.Father = father;

        var s = person.PrintToString();
        
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WhenSerializingDictionary()
    {
        person.Addresses = new Dictionary<int, string>
        {
            { 1, "London" },
            { 2, "New York" },
            { 3, "Moscow" }
        };

        var s = person.PrintToString();

        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldWorkCorrectly_WhenSerializingEnumerable()
    {
        person.Children =
        [
            new Person { Name = "Natasha", Age = 8, DateOfBirth = new DateTime(2002, 9 , 9) },
            new Person { Name = "Pasha", Age = 8, DateOfBirth = new DateTime(2002, 9 , 9) },
        ];

        var s = person.PrintToString();

        Approvals.Verify(s);
    }
}