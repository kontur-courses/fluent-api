
using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

public class ObjectPrinterTests
{
    [Test]
    public void ExcludingTypeTest()
    {
        var person = new Person { Name = "Alex", Age = 25, Id = Guid.NewGuid() };
        var printer = ObjectPrinter.For<Person>()
            .Excluding<Guid>()
            .Excluding<int>();
        var printed = printer.PrintToString(person);
        Assert.That(printed, Does.Not.Contain("Id"));
        Assert.That(printed, Does.Not.Contain("Age"));
        Assert.That(printed, Contains.Substring("Name"));
    }

    [Test]
    public void ExcludingPropertyTest()
    {
        var person = new Person { Name = "Alex", Age = 25, Id = Guid.NewGuid() };
        var printer = ObjectPrinter.For<Person>()
            .Excluding(p => p.Age);
        var printed = printer.PrintToString(person);
        Assert.That(printed, Does.Not.Contain("Age"));
        Assert.That(printed, Contains.Substring("Name"));
    }
    
    [Test]
    public void CustomTypeSerializationTest()
    {
        var person = new Person { Name = "Alex", Age = 25, Id = Guid.NewGuid() };
        var printer = ObjectPrinter.For<Person>()
            .Printing<int>(i => $"Number is {i}");
        var printed = printer.PrintToString(person);
        Assert.That(printed, Contains.Substring("Number is 25"));
    }
    
    [Test]
    public void TrimTest()
    {
        var person = new Person { Name = "Alex", Age = 25, Id = Guid.NewGuid() };
        var printer = ObjectPrinter.For<Person>()
            .SelectMember(p => p.Name).AsString().Trim(3);
        var printed = printer.PrintToString(person);
        Assert.That(printed, Contains.Substring("Name = Ale"));
    }
    
    [Test]
    public void Printing_WithCulture_ForIFormattableTypes()
    {
        var obj = new Format()
        {
            Value = 1234.56,
            Date = new DateTime(2024, 5, 25, 13, 45, 0)
        };
    
        var printer = ObjectPrinter.For<Format>()
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            .Printing<DateTime>().Using(new CultureInfo("ru-RU"));
    
        string result = printer.PrintToString(obj);
        
        Assert.That(result, Does.Contain("1234.56"));
        Assert.That(result, Does.Contain("25.05.2024"));
        Assert.That(result, Does.Not.Contain("5/25/2024"));
    }
    
    [Test]
    public void ExtensionWithConfig()
    {
        var person = new Person { Name = "Alex", Age = 25 };
    
        var printed = person.PrintToString(cfg => cfg.Excluding(p => p.Age));
    
        Assert.That(printed, Does.Not.Contain("Age"));
        Assert.That(printed, Contains.Substring("Alex"));
    }

    [Test]  
    public void DefaultSerializationIncludesPropertiesAndFields()
    {
        var person = new Person { FirstName = "Ivanov", Name = "Alex", Age = 25 };
        var printer = ObjectPrinter.For<Person>();
        var printed = printer.PrintToString(person);

        Assert.That(printed, Contains.Substring("FirstName"));
        Assert.That(printed, Contains.Substring("Ivanov"));
    }
    
    [Test]
    public void ArraySerializeTest()
    {
        var obj = new Container
        {
            Array = new[] { 1, 2, 3 }
        };

        var printer = ObjectPrinter.For<Container>();
        var result = printer.PrintToString(obj);

        Assert.That(result, Contains.Substring("Array"));
        Assert.That(result, Contains.Substring("[0] = 1"));
        Assert.That(result, Contains.Substring("[1] = 2"));
        Assert.That(result, Contains.Substring("[2] = 3"));
    }
    
    [Test]
    public void ListSerializeTest()
    {
        var obj = new Container
        {
            List = new List<string> { "Alex", "Ivan" }
        };

        var printer = ObjectPrinter.For<Container>();
        var result = printer.PrintToString(obj);

        Assert.That(result, Contains.Substring("List"));
        Assert.That(result, Contains.Substring("[0] = Alex"));
        Assert.That(result, Contains.Substring("[1] = Ivan"));
    }
    
    [Test]
    public void DictionarySerializationTest()
    {
        var obj = new Container
        {
            Dict = new Dictionary<string, int>
            {
                ["Alex"] = 19,
                ["Ivan"] = 21
            }
        };

        var printer = ObjectPrinter.For<Container>();
        var result = printer.PrintToString(obj);

        Assert.That(result, Contains.Substring("Dict"));
        Assert.That(result, Contains.Substring("[Alex] = 19"));
        Assert.That(result, Contains.Substring("[Ivan] = 21"));
    }
    
    [Test]
    public void PrettySerializationTest()
    {
        var person = new Person { Name = "Alex", Age = 25, FirstName = "Ivanov" };
        var printer = ObjectPrinter.For<Person>();
        var printed = printer.PrintToString(person);

        var expected = "Person\n\tName = Alex\n\tAge = 25\n\tScore = 0\n\tId = Guid\n\tFirstName = Ivanov";
        
        Assert.That(
            printed.Replace("\r\n", "\n").Trim(),
            Is.EqualTo(expected.Trim())
        );
    }

    [Test]
    public void TrimGlobalTest()
    {
        var person = new Person { Name = "Alexandr", Age = 25, FirstName = "Ivanov" };
        var printer = ObjectPrinter.For<Person>().TrimStringsGlobal(4);
        var result = printer.PrintToString(person);
        Assert.That(result, Contains.Substring("Alex"));
        Assert.That(result, Contains.Substring("Ivan"));
    }
}