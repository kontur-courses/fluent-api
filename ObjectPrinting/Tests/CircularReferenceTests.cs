using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class CircularReferenceTests
{
    [Test]
    public void PrintToString_WithCircularReference_ShouldNotStackOverflow()
    {
        var person1 = new Person { Name = "Person1" };
        var person2 = new Person { Name = "Person2" };
        
        person1.Parent = person2;
        person2.Parent = person1;

        var printer = ObjectPrinter.For<Person>();

        Action act = () => printer.PrintToString(person1);

        act.Should().NotThrow<StackOverflowException>();
    }

    [Test]
    public void PrintToString_WithSelfReference_ShouldHandleCorrectly()
    {
        var person = new Person { Name = "SelfReferencing" };
        person.Parent = person; 

        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Contain("Cyclic reference").And.Contain("SelfReferencing");
    }

    [Test]
    public void PrintToString_WithComplexCircularReference_ShouldHandleCorrectly()
    {
        var company = new Company { Name = "Tech Corp" };
        var department = new Department { Name = "Development", Company = company };
        var employee = new Employee { Name = "John", Department = department };
        
        company.Departments = new List<Department> { department };
        department.Employees = new List<Employee> { employee };
        employee.Manager = employee;

        var printer = ObjectPrinter.For<Company>();

        Action act = () => printer.PrintToString(company);

        act.Should().NotThrow<StackOverflowException>();
    }

    [Test]
    public void CircularReference_WithConfiguration_ShouldStillWork()
    {
        var person1 = new Person { Name = "Person1", Age = 30 };
        var person2 = new Person { Name = "Person2", Age = 25 };
        
        person1.Parent = person2;
        person2.Parent = person1;

        var printer = ObjectPrinter.For<Person>()
            .ConfigureType<int>().WithSerialization(i => $"{i} years")
            .ConfigureProperty(p => p.Name).Trim(0, 6)
            .ExcludeProperty(p => p.BirthDate);

        Action act = () => printer.PrintToString(person1);

        act.Should().NotThrow<StackOverflowException>();
    }
}

public class Company
{
    public string Name { get; set; }
    public List<Department> Departments { get; set; }
}

public class Department
{
    public string Name { get; set; }
    public Company Company { get; set; }
    public List<Employee> Employees { get; set; }
}

public class Employee
{
    public string Name { get; set; }
    public Department Department { get; set; }
    public Employee Manager { get; set; }
    public decimal Salary { get; set; }
}