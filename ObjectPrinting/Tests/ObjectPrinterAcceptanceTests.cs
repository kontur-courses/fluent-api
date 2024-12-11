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
    [Test]
    [UseReporter]
    public void Demo()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 1.81, Status = Status.Student };
        var printer = ObjectPrinter.For<Person>();
        var s = printer.PrintToString(person);
            
        Approvals.Verify(s);
    }
        
    [Test]
    [UseReporter]
    public void ExcludeTypeFromSerialization()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 1.81, Status = Status.Student};
        var printer = ObjectPrinter
            .For<Person>()
            .Excluding<Status>();   //1. Исключить из сериализации свойства определенного типа
        var s = printer.PrintToString(person);
            
        Approvals.Verify(s);
    }
        
    [Test]
    [UseReporter]
    public void CustomSerializationForSpecificType()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 1.81, Status = Status.Student };
        var printer = ObjectPrinter
            .For<Person>()
            .Printing<Status>().Using(i => ((int)i).ToString());   //2. Указать альтернативный способ сериализации для определенного типа
        var s = printer.PrintToString(person);
            
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter]
    public void SerializationWithCustomCulture()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 1.81, Status = Status.Student };
        var printer = ObjectPrinter
            .For<Person>()
            .Printing<double>().Using(CultureInfo.InvariantCulture);    //3. Для числовых типов указать культуру
        var s = printer.PrintToString(person);
            
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter]
    public void TrimmingStringProperties()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 1.81, Status = Status.Student };
        var printer = ObjectPrinter
            .For<Person>()
            .Printing(p => p.Name)   //4. Настроить сериализацию конкретного свойства
            .TrimmedToLength(2);    //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
        var s = printer.PrintToString(person);
            
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter]
    public void ExcludeSpecificProperty()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 1.81, Status = Status.Student };
        var printer = ObjectPrinter
            .For<Person>()
            .Excluding(p => p.Height);  //6. Исключение из сериализации конкретного свойства/поля
        var s = printer.PrintToString(person);
            
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter]
    public void CombineSerializationRules()
    {
        var person = new Person { Name = "Benjamin", Age = 19, Height = 1.81, Status = Status.Student };
        var printer = ObjectPrinter
            .For<Person>()
            .Excluding<Guid>()
            .Printing<Status>().Using(i => ((int)i).ToString())
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            .Printing(p => p.Name).TrimmedToLength(5)
            .Excluding(p => p.Age);
        var s = printer.PrintToString(person);
            
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter]
    public void DefaultSerializationUsingExtensionMethod()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 1.81, Status = Status.Student };
        var s = person.PrintToString();  //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter]
    public void DefaultSerializationWithConfiguration()
    {
        var person = new Person { Name = "Alex", Age = 19, Height = 1.81, Status = Status.Student };
        var s = person.PrintToString(s => s.Excluding(p => p.Age));  //8. ...с конфигурированием
            
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter]
    public void HandleCyclicReferencesInObjectGraph()
    {
        var company = new Company { Name = "TechCorp" };
        var department1 = new Department { Name = "Headquarters", ParentCompany = company };
        var department2 = new Department { Name = "Headquarters", ParentCompany = company, SubDepartment = department1 };
        
        department1.SubDepartment = department2;
        company.MainDepartment = department2;
        
        var printer = ObjectPrinter.For<Company>();
        var s = printer.PrintToString(company);
        
        Approvals.Verify(s);
    }
    
    [Test]
    [UseReporter]
    public void SerializeArrayOfObjects()
    {
        var rooms = new[]
        {
            new Room { Name = "Living Room", Area = 20.5 },
            new Room { Name = "Bedroom", Area = 15.3 },
            new Room { Name = "Kitchen", Area = 10.0 }
        };

        var flat = new Flat
        {
            Floor = 5,
            ApartmentNumber = 101,
            Rooms = rooms
        };

        var printer = ObjectPrinter.For<Flat>();
        var serializedFlat = printer.PrintToString(flat);
        
        Approvals.Verify(serializedFlat);
    }
    
    [Test]
    [UseReporter]
    public void SerializeListOfObjects()
    {
        var rooms = new List<Room>
        {
            new Room { Name = "Living Room", Area = 20.5 },
            new Room { Name = "Bedroom", Area = 15.3 },
            new Room { Name = "Kitchen", Area = 10.0 }
        };

        var flat = new Flat
        {
            Floor = 5,
            ApartmentNumber = 101,
            Rooms = rooms
        };

        var printer = ObjectPrinter.For<Flat>();
        var serializedFlat = printer.PrintToString(flat);
        
        Approvals.Verify(serializedFlat);
    }
    
    [Test]
    [UseReporter]
    public void SerializeDictionaryObject()
    {
        var building = new Building
        {
            Name = "Central Plaza",
            Floors = new Dictionary<int, List<Room>>
            {
                { 1, [new Room { Name = "Lobby", Area = 100.0 }] },
                { 2, [new Room { Name = "Office 201", Area = 50.0 }, new Room { Name = "Office 202", Area = 45.0 }] },
                { 3, [new Room { Name = "Conference Room", Area = 120.0 }] }
            }
        };

        var printer = ObjectPrinter.For<Building>();
        var serializedBuilding = printer.PrintToString(building);
        
        Approvals.Verify(serializedBuilding);
    }
}