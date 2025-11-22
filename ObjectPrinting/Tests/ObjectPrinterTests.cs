using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Solved;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterTests
{
    private Person? testPerson;

    [SetUp]
    public void SetUp()
    {
        testPerson = new Person
        {
            Id = Guid.NewGuid(),
            Name = "Alexandra",
            Height = 152,
            Age = 52
        };
    }

    [Test]
    public void PrintToString_SimpleObject_ReturnsAllProperties()
    {
        var result = testPerson.PrintToString();

        result.Should().Contain("Person")
            .And.Contain("Name = \"Alexandra\"")
            .And.Contain("Age = 52")
            .And.Contain("Height = 152")
            .And.Contain("Id = ");
    }

    [Test]
    public void PrintToString_ExcludingGuid_ShouldExcludeIdProperty()
    {
        var result = testPerson.PrintToString(config => config.Excluding<Guid>());
        
        result.Should().NotContain("Id");
    }

    [Test]
    public void PrintToString_ExcludingDouble_ShouldExcludeHeightProperty()
    {
        var result = testPerson.PrintToString(config => config.Excluding<double>());
        
        result.Should().NotContain("Height");
    }

    [Test]
    public void PrintToString_ExcludingInt_ShouldExcludeAgeProperty()
    {
        var result = testPerson.PrintToString(config => config.Excluding<int>());
        
        result.Should().NotContain("Age");
    }

    [Test]
    public void PrintToString_ExcludingString_ShouldExcludeNameProperty()
    {
        var result = testPerson.PrintToString(config => config.Excluding<string>());
        
        result.Should().NotContain("Name");
    }
    
    [Test]
    public void PrintToString_ExcludingSpecificMember_ShouldExcludeOnlyThatProperty()
    {
        var result = testPerson.PrintToString(config =>
            config.Excluding(p => p.Height));

        result.Should().NotContain("Height")
            .And.Contain("Name = \"Alexandra\"")
            .And.Contain("Age = 52")
            .And.Contain("Id = ");
    }

    [Test]
    public void PrintToString_ExcludingMultipleMembers_ShouldExcludeAllSpecified()
    {
        var result = testPerson.PrintToString(config => config
            .Excluding(p => p.Height)
            .Excluding(p => p.Name));

        result.Should().NotContain("Height").And.NotContain("Name")
            .And.Contain("Age = 52")
            .And.Contain("Id = ");
    }
    
    [Test]
    public void PrintToString_ExcludingMultipleTypesAndMembers_ShouldExcludeAllSpecified()
    {
        var result = testPerson.PrintToString(config => config
            .Excluding<Guid>()
            .Excluding<double>()
            .Excluding(p => p.Name));

        result.Should()
            .NotContain("Id").And
            .NotContain("Height").And
            .NotContain("Name").And
            .Contain("Age = 52");
    }
        
    [Test]
    public void PrintToString_PrintingIntWithCustomSerializer_ShouldUseCustomFormat()
    {
        var result = testPerson.PrintToString(config =>
            config.Printing<int>().Using(i => $"{i} years"));
        
        result.Should().Contain("Age = 52 years");
    }

    [Test]
    public void PrintToString_PrintingDoubleWithCustomSerializer_ShouldUseCustomFormat()
    {
        var result = testPerson.PrintToString(config =>
            config.Printing<double>().Using(d => $"{d:F1} cm"));

        result.Should().Contain("Height = 152,0 cm");
    }

    [Test]
    public void PrintToString_PrintingStringWithCustomSerializer_ShouldUseCustomFormat()
    {
        var result = testPerson.PrintToString(config =>
            config.Printing<string>().Using(s => s.ToUpper()));

        result.Should().Contain("Name = ALEXANDRA");
    }
    
    [Test]
    public void PrintToString_TrimmedToSmallLength_ShouldTrimNameProperty()
    {
        var result = testPerson.PrintToString(config =>
            config.Printing(p => p.Name).TrimmedToLength(3));

        result.Should().Contain("Name = \"Ale\"")
            .And.NotContain("Alexandra");
    }

    [Test]
    public void PrintToString_TrimmedToLengthLongLength_ShouldNotChangeName()
    {
        var result = testPerson.PrintToString(config =>
            config.Printing(p => p.Name).TrimmedToLength(20));

        result.Should().Contain("Name = \"Alexandra\"");
    }
    
    [Test]
    public void PrintToString_PrintingWithRussianCulture_ShouldFormatDoubleWithComma()
    {
        var result = testPerson.PrintToString(config =>
            config.Printing<double>().Using(CultureInfo.GetCultureInfo("ru-RU")));

        result.Should().ContainAny("Height = 180,5", "Height = 152");
    }
    
    [Test]
    public void PrintToString_PrintingWithUsCulture_ShouldFormatDoubleWithDot()
    {
        var result = testPerson.PrintToString(config =>
            config.Printing<double>().Using(CultureInfo.GetCultureInfo("en-US")));

        result.Should().ContainAny("Height = 180.5", "Height = 152");
    }
    
    [Test]
    public void PrintToString_WithIntArray_ShouldPrintArray()
    {
        int[] numbers = { 1, 2, 3 };

        var result = numbers.PrintToString();

        result.Should().Contain("Int32[]")
            .And.Contain("[0] = 1")
            .And.Contain("[1] = 2")
            .And.Contain("[2] = 3");
    }

    [Test]
    public void PrintToString_WithStringArray_ShouldPrintArray()
    {
        string[] names = { "Sasha", "Misha", "Grisha" };

        var result = names.PrintToString();

        result.Should().Contain("String[]")
            .And.Contain("[0] = \"Sasha\"")
            .And.Contain("[1] = \"Misha\"")
            .And.Contain("[2] = \"Grisha\"");
    }

    [Test]
    public void PrintToString_WithPersonList_ShouldPrintList()
    {
        var people = new List<Person>
        {
            new Person { Name = "Sasha", Age = 52 },
            new Person { Name = "Misha", Age = 30 }
        };

        var result = people.PrintToString();

        result.Should().Contain("List`1")
            .And.Contain("[0] =")
            .And.Contain("[1] =")
            .And.Contain("Name = \"Sasha\"")
            .And.Contain("Name = \"Misha\"")
            .And.Contain("Age = 52")
            .And.Contain("Age = 30");
    }

    [Test]
    public void PrintToString_WithDictionary_ShouldPrintDictionary()
    {
        var dict = new Dictionary<string, int>
        {
            ["Sasha"] = 52,
            ["Masha"] = 30
        };

        var result = dict.PrintToString();

        result.Should().Contain("Dictionary`2")
            .And.Contain("[\"Sasha\"] = 52")
            .And.Contain("[\"Masha\"] = 30");
    }
    
    [Test]
    public void PrintToString_WithCycleReferenceInList_ShouldDetect()
    {
        var list = new List<object>();
        list.Add(list); 

        var result = list.PrintToString();

        result.Should().Contain("Cyclic reference detected");
        Assert.DoesNotThrow(() => list.PrintToString());
    }

    [Test]
    public void PrintToString_WithNullObject_ReturnsNullString()
    {
        Person nullPerson = null;
        var result = nullPerson.PrintToString().Trim();

        result.Should().Be("null");
    }

    [Test]
    public void PrintToString_WithNullProperty_HandlesNull()
    {
        var personWithNull = new Person { Name = null, Age = 52 };
        var result = personWithNull.PrintToString();

        result.Should().Contain("Name = null")
            .And.Contain("Age = 52");
    }
    
    [Test]
    public void PrintToString_WithNestedObjectsInList_ShouldSerializeRecursively()
    {
        var people = new List<Person>
        {
            new() { Name = "Sasha", Age = 45 },
            new() { Name = "Masha", Age = 30 }
        };

        var result = people.PrintToString();

        result.Should().Contain("List`1")
            .And .Contain("[0] =")
            .And .Contain("[1] =")
            .And .Contain("Person")
            .And .Contain("Name = \"Sasha\"")
            .And .Contain("Age = 45")
            .And .Contain("Name = \"Masha\"")
            .And .Contain("Age = 30");
    }
    
    [Test]
    public void PrintToString_WithNestedObject_ShouldContainNestedProperties()
    {
        var office = new Office
        {
            Address = "Moskovskaya",
            Manager = new Person { Name = "Sasha", Age = 44 }
        };

        var company = new Company
        {
            Name = "Kontur",
            Office = office
        };

        var result = company.PrintToString();

        result.Should().Contain("Name = \"Kontur\"")
            .And.Contain("Office")
            .And.Contain("Office =")
            .And.Contain("Office")
            .And.Contain("Address = \"Moskovskaya\"")
            .And.Contain("Manager =")
            .And.Contain("Person")
            .And.Contain("Name = \"Sasha\"")
            .And .Contain("Age = 44");
    }

    [Test]
    public void PrintToString_WithNestedObject_ShouldIncludeAllLevels()
    {
        var root = new TreeNode
        {
            Value = "Root",
            Child = new TreeNode
            {
                Value = "ChildOfRoot",
                Child = new TreeNode
                {
                    Value = "Leaf"
                }
            }
        };

        var result = root.PrintToString();

        result.Should() .Contain("TreeNode")
            .And.Contain("Value = \"Root\"")
            .And.Contain("Value = \"ChildOfRoot\"")
            .And.Contain("Value = \"Leaf\"");
    }
    
    private class Company
    {
        public string Name { get; set; }
        public Office Office { get; set; }
    }

    private class Office
    {
        public string Address { get; set; }
        public Person Manager { get; set; }
    }

    private class TreeNode
    {
        public string Value { get; set; }
        public TreeNode Child { get; set; }
    }
}
