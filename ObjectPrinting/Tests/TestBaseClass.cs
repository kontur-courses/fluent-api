using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

public class TestBase
{
    protected Person TestPerson { get; private set; }
    protected Person PersonWithCyclicReference { get; private set; }

    [OneTimeSetUp]
    public virtual void SetUp()
    {
        TestPerson = new Person
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Age = 30,
            Height = 180.5,
            Email = "john.doe@example.com"
        };

        var parent = new Person { Name = "Parent" };
        var child = new Person { Name = "Child", Parent = parent };
        parent.Children.Add(child);
        PersonWithCyclicReference = parent;
    }

    protected List<Person> CreateTestPeopleList() => new()
    {
        new Person { Name = "First", Age = 20 },
        new Person { Name = "Second", Age = 25 }
    };

    protected string[] CreateTestStringArray() => ["apple", "banana", "cherry"];

    protected Dictionary<string, int> CreateTestDictionary() => new()
    {
        ["first"] = 1,
        ["second"] = 2,
        ["third"] = 3
    };
}