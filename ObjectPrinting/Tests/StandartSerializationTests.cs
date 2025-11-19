using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class StandartSerializationTests : TestBase
{
    [Test]
    public void PrintToString_SimpleObject_ReturnsCorrectFormat()
    {
        var result = TestPerson.PrintToString();

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Person");
        result.Should().Contain("Name = John Doe");
        result.Should().Contain("Age = 30");
        result.Should().Contain("Height = 180.5");
    }

    [Test]
    public void PrintToString_WithNullProperty_HandlesNullCorrectly()
    {
        var person = new Person { Name = null };
        var result = person.PrintToString();

        result.Should().Contain("Name = null");
    }

    [Test]
    public void PrintToString_WithInheritance_IncludesAllProperties()
    {
        var testEmployee = new Employee
        {
            Name = "Jane",
            Age = 25,
            Position = "Developer",
            Salary = 50000.50m
        };
        
        var result = testEmployee.PrintToString();

        result.Should()
            .Contain("Employee")
            .And.Contain("Position = Developer")
            .And.Contain("Salary = 50000.50")
            .And.Contain("Name = Jane")
            .And.Contain("Age = 25");
    }

    [Test]
    public void PrintToString_ExtensionMethodWithoutConfig_Works()
    {
        var result = TestPerson.PrintToString();

        result.Should().NotBeNullOrEmpty().And.Contain("Person");
    }

    [Test]
    public void PrintToString_ExtensionMethodWithConfig_AppliesConfiguration()
    {
        var result = TestPerson.PrintToString(config =>
            config.ExcludeProperty(p => p.Age)
                .SerializeProperty(p => p.Name).TrimTo(2));

        result.Should().NotContain("Age = 30").And.Contain("Name = Jo");
    }

    [Test]
    public void PrintToString_WithDateTime_SerializesCorrectly()
    {
        var testClass = new { Date = new DateTime(2023, 12, 31) };
        var result = testClass.PrintToString();

        result.Should().Contain("Date = ");
    }

    [Test]
    public void PrintToString_WithEnum_SerializesCorrectly()
    {
        var testClass = new { Status = System.DateTimeKind.Utc };
        var result = testClass.PrintToString();

        result.Should().Contain("Status = Utc");
    }

    [Test]
    public void PrintToString_WithNestedObjects_SerializesRecursively()
    {
        var testCompany = new Company
        {
            Name = "Test Corp",
            CEO = new Employee { Name = "CEO", Age = 45 }
        };
        
        var result = testCompany.PrintToString();

        result.Should()
            .Contain("Company")
            .And.Contain("CEO = Employee")
            .And.Contain("Name = CEO")
            .And.Contain("Age = 45");
    }

    [Test]
    public void PrintToString_WithDifferentNumericTypes_UsesCorrectFormatting()
    {
        var numericObject = new
        {
            IntValue = 42,
            DoubleValue = 3.14159,
            DecimalValue = 123.456m,
            FloatValue = 2.718f
        };

        var result = numericObject.PrintToString();

        result.Should()
            .Contain("IntValue = 42")
            .And.Contain("DoubleValue = 3.14159")
            .And.Contain("DecimalValue = 123.456")
            .And.Contain("FloatValue = 2.718");
    }

    [Test]
    public void PrintToString_EmptyObject_ReturnsTypeName()
    {
        var result = new object().PrintToString();
        result.Should().Contain("Object");
    }
    
    [Test]
    public void PrintToString_WithNestingLevelLimit_StopsAtSpecifiedDepth()
    {
        var nestedObject = new NestedContainer
        {
            Value = "Level 1",
            Child = new NestedContainer
            {
                Value = "Level 2", 
                Child = new NestedContainer
                {
                    Value = "Level 3",
                    Child = new NestedContainer { Value = "Level 4" }
                }
            }
        };
        
        var result = nestedObject.PrintToString(2);

        result.Should().Contain("Level 1");
        result.Should().Contain("Level 2"); 
        result.Should().NotContain("Level 3");
        result.Should().NotContain("Level 4");
    }
}