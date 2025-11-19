using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class CombinedScenariosTests
{
    [Test]
    public void CombinedConfigurations_ShouldWorkTogether()
    {
        var person = new Person 
        { 
            Name = "Alexander", 
            Age = 25, 
            Height = 180.5,
            YearOfBirth = 1998,
            Friends = new List<string> { "John", "Michael", "Sarah" }
        };

        var printer = ObjectPrinter.For<Person>()
            .ExcludeType<string>()
            .ConfigureType<int>().WithSerialization(i => $"{i} years")
            .ConfigureType<double>().WithCulture(CultureInfo.InvariantCulture)
            .ConfigureProperty(p => p.Age).WithSerialization(age => $"Возраст: {age}")
            .ConfigureCollection<List<string>>()
                .Trim(0, 4);

        var result = printer.PrintToString(person);

        result.Should().NotContain("Alexander")
              .And.Contain("Age = Возраст: 25")
              .And.Contain("YearOfBirth = 1998 years")
              .And.Contain("John") 
              .And.Contain("Mich"); 
    }

    [Test]
    public void PropertySerialization_ShouldHavePriorityOverTypeSerialization()
    {
        var person = new Person { Name = "Alex", Age = 25, YearOfBirth = 1998 };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureType<int>().WithSerialization(i => $"{i} years")
            .ConfigureProperty(p => p.Age).WithSerialization(age => $"Возраст: {age}");

        var result = printer.PrintToString(person);

        result.Should().Contain("Age = Возраст: 25")
              .And.Contain("YearOfBirth = 1998 years");  
    }

    [Test]
    public void CollectionConfig_ShouldOverrideTypeConfig()
    {
        var person = new Person 
        { 
            Name = "Alex",
            Scores = new List<int> { 95, 87, 92 },
            YearOfBirth = 1998
        };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureType<int>().WithSerialization(i => $"#{i}#")
            .ConfigureCollection<List<int>>()
                .WithItemSerialization(score => $"Score: {score}");

        var result = printer.PrintToString(person);

        result.Should().Contain("Score: 95")  
              .And.Contain("Score: 87")
              .And.Contain("Score: 92")
              .And.Contain("YearOfBirth = #1998#");  
    }

    [Test]
    public void ComplexObject_WithAllFeatures_ShouldSerializeCorrectly()
    {
        var company = new Company 
        { 
            Name = "Tech Corp",
            Departments = new List<Department>
            {
                new Department 
                { 
                    Name = "Development",
                    Employees = new List<Employee>
                    {
                        new Employee { Name = "John", Salary = 50000.0m },
                        new Employee { Name = "Sarah", Salary = 60000.0m }
                    }
                }
            }
        };

        var printer = ObjectPrinter.For<Company>()
            .ConfigureType<decimal>().WithCulture(new CultureInfo("en-US"))
            .ConfigureProperty(p => p.Name).WithSerialization(name => $"Company: {name}")
            .ConfigureCollection<List<Employee>>()
            .WithItemSerialization(emp => 
            {
                if (emp is Employee employee)
                {
                    return $"{employee.Name} (${employee.Salary.ToString("F2", CultureInfo.InvariantCulture)})";
                }
                return emp?.ToString() ?? "null";
            });

        var result = printer.PrintToString(company);

        result.Should().Contain("Company: Tech Corp")
            .And.Contain("John ($50000.00)")
            .And.Contain("Sarah ($60000.00)");
    }
}