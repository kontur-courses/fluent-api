using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class PrintingConfigTests
{
    private Person CreatePerson(string name = "Alexander", int age = 30)
    {
        return new Person
        {
            Id = Guid.Empty,
            Name = name,
            Salary = 1234.5,
            Age = age,
            BirthDate = new DateTime(2000, 1, 2),
            Description = "Some long description"
        };
    }

    private PersonGroup CreateGroup()
    {
        return new PersonGroup
        {
            GroupName = "DevTeam",
            Leader = CreatePerson("Leader", 30),
            Members =
            [
                CreatePerson("Alice", 25),
                CreatePerson("Bob", 28)
            ],
            Scores = [322, 67, 35],
            ScoresList = [3.0, 4.2, 6.1],
            ScoresDictionary = new Dictionary<string, Dictionary<string, int>>
            {
                ["User1"] = new()
                {
                    ["Score"] = 10,
                    ["Level"] = 1
                },
                ["User2"] = new()
                {
                    ["Score"] = 5,
                    ["Level"] = 2
                }
            }
        };
    }
    
    [Test]
    public void ExcludeType_Remove_PropertiesType()
    {
        var person = CreatePerson();
        var config = new PrintingConfig<Person>();

        config.For<int>()
              .ExcludeType()
              .End();

        var result = config.PrintToString(person);

        result.Should().NotContain("Age =");
        result.Should().NotContain("30");
        result.Should().Contain("Name = Alexander");
        result.Should().Contain("Salary = 1234,5");
    }

    [Test]
    public void SetSerializer_SetSerialization_ForType()
    {
        var person = CreatePerson();
        var config = new PrintingConfig<Person>();

        config.For<string>()
              .SetSerializer(i => $"str:{i}")
              .End();

        var result = config.PrintToString(person);

        result.Should().Contain("Name = str:Alexander");
        result.Should().Contain("Description = str:Some long description");
    }

    [Test]
    public void UsingCulture_ChangeCulture_ForPropertiesType()
    {
        var person = CreatePerson();
        var config = new PrintingConfig<Person>();

        config.For<double>()
              .UsingCulture(new CultureInfo("en-US"))
              .End();

        var result = config.PrintToString(person);

        result.Should().Contain("Salary = 1234.5");
    }

    [Test]
    public void PropertySetSerializer_SetSerialization_ForProperty()
    {
        var person = CreatePerson();
        var config = new PrintingConfig<Person>();

        config.For(p => p.Age)
              .SetSerializer(a => $"AgeIs:{a}")
              .End();

        var result = config.PrintToString(person);

        result.Should().Contain("Age = AgeIs:30");
        result.Should().Contain("Description = Some long description");
    }

    [Test]
    public void PropertyExclude_Remove_Properties()
    {
        var person = CreatePerson();
        var config = new PrintingConfig<Person>();

        config.For(p => p.Description)
              .ExcludeProperty()
              .End();

        var result = config.PrintToString(person);

        result.Should().NotContain("Description =");
        result.Should().Contain("Name = Alexander");
    }

    [Test]
    public void StringTrimToLength_Trim_StringProperty()
    {
        var person = CreatePerson();
        var config = new PrintingConfig<Person>();

        config.For(p => p.Name)
              .TrimToLength(5)
              .End();

        var result = config.PrintToString(person);

        result.Should().Contain("Name = Alexa");
        result.Should().NotContain("Alexander");
    }

    [Test] public void StringTrimToLengthAndSetSerializerShould_BothApplied()
    {
        var person = CreatePerson();
        var config = new PrintingConfig<Person>();

        config.For(p => p.Name)
              .TrimToLength(5)
              .SetSerializer(s => s.ToUpperInvariant())
              .End();

        var result = config.PrintToString(person);

        result.Should().Contain("Name = ALEXA");
    }

    [Test]
    public void ExcludeType_HavePriority_OverPropertySetSerializer()
    {
        var person = CreatePerson();
        var config = new PrintingConfig<Person>();

        config.For(p => p.Age)
              .SetSerializer(a => $"AgeIs:{a}")
              .End();

        config.For<int>()
              .ExcludeType()
              .End();

        var result = config.PrintToString(person);

        result.Should().NotContain("Age =");
        result.Should().NotContain("AgeIs:30");
    }

    [Test]
    public void Collections_SerializeCorrectly()
    {
        var group = CreateGroup();
        var config = new PrintingConfig<PersonGroup>();

        var result = config.PrintToString(group);

        result.Should().Contain("PersonGroup");
        result.Should().Contain("GroupName = DevTeam");
        
        result.Should().Contain("Members = Person[]");
        result.Should().Contain("Name = Alice");
        result.Should().Contain("Name = Bob");
        
        result.Should().Contain("Scores = Int32[]");
        result.Should().Contain("322");
        result.Should().Contain("67");
        result.Should().Contain("35");
        
        result.Should().Contain("ScoresList = List`1");
        result.Should().Contain("3");
        result.Should().Contain("4,2");
        result.Should().Contain("6,1");
        
        result.Should().Contain("ScoresDictionary = Dictionary`2");
        result.Should().Contain("Key = User1");
        result.Should().Contain("Score");
        result.Should().Contain("10");
        result.Should().Contain("Key = User2");
        result.Should().Contain("5");
    }

    [Test]
    public void ExcludeType_WorkInsideCollections()
    {
        var group = CreateGroup();
        var config = new PrintingConfig<PersonGroup>();

        config.For<int>()
              .ExcludeType()
              .End();

        var result = config.PrintToString(group);

        result.Should().NotContain("Age =");
        result.Should().Contain("Scores = Int32[]");
        result.Should().NotContain("322");
        result.Should().NotContain("67");
        result.Should().NotContain("35");
    }

    [Test]
    public void PropertySetSerializer_Work_ForNestedProperty()
    {
        var group = CreateGroup();
        var config = new PrintingConfig<PersonGroup>();

        config.For(g => g.Leader.Name)
              .SetSerializer(s => $"LEADER:{s}")
              .End();

        var result = config.PrintToString(group);

        result.Should().Contain("Name = LEADER:Leader");
        result.Should().Contain("Name = LEADER:Alice");
        result.Should().Contain("Name = LEADER:Bob");
    }
    
    [Test]
    public void TypeSetSerializer_Work_InsideCollections()
    {
        var group = CreateGroup();
        var config = new PrintingConfig<PersonGroup>();

        config.For<int>()
            .SetSerializer(i => $"INT:{i}")
            .End();

        var result = config.PrintToString(group);

        result.Should().Contain("Age = INT:30");
        result.Should().Contain("Age = INT:25");
        result.Should().Contain("Age = INT:28");
        result.Should().Contain("Scores = Int32[]");
        result.Should().Contain("INT:322");
        result.Should().Contain("INT:67");
        result.Should().Contain("INT:35");
        result.Should().Contain("ScoresDictionary = Dictionary`2");
        result.Should().Contain("INT:10");
        result.Should().Contain("INT:1");
        result.Should().Contain("INT:5");
        result.Should().Contain("INT:2");
    }
    
    [Test]
    public void UsingCulture_Work_InsideCollections()
    {
        var group = CreateGroup();
        var config = new PrintingConfig<PersonGroup>();

        config.For<double>()
            .UsingCulture(new CultureInfo("en-US"))
            .End();

        var result = config.PrintToString(group);

        result.Should().Contain("Salary = 1234.5");
        result.Should().Contain("ScoresList = List`1");
        result.Should().Contain("3");
        result.Should().Contain("4.2");
        result.Should().Contain("6.1");
    }   
    
    [Test]
    public void StringTrimToLength_Work_InsideCollections()
    {
        var group = CreateGroup();
        var config = new PrintingConfig<PersonGroup>();

        config.For(p => p.Leader.Name)
            .TrimToLength(1)
            .End();

        var result = config.PrintToString(group);

        result.Should().Contain("Name = L");
        result.Should().Contain("Name = A");
        result.Should().Contain("Name = B");
    }
    
    [Test]
    public void ExcludeCollections_NotSerializeCollectionsWithElements()
    {
        var group = CreateGroup();
        var config = new PrintingConfig<PersonGroup>();

        config.For<int[]>()
            .ExcludeType()
            .End();

        var result = config.PrintToString(group);

        result.Should().NotContain("Scores = Int32[]");
        result.Should().NotContain("322");
        result.Should().NotContain("67");
        result.Should().NotContain("35");
    }
    
    [Test]
    public void ExcludeProperty_Work_ForNestedProperty()
    {
        var group = CreateGroup();
        var config = new PrintingConfig<PersonGroup>();

        config.For(x=>x.Leader.Age)
            .ExcludeProperty()
            .End();

        var result = config.PrintToString(group);

        result.Should().NotContain("Age = 30");
        result.Should().NotContain("Age = 25");
        result.Should().NotContain("Age = 28");
    }
    [Test]
    public void CircularDependency_NotCauseStackOverflow()
    {
        var list = new List<object>();
        list.Add("first");
        list.Add(list);

        var config = new PrintingConfig<List<object>>();

        var result = config.PrintToString(list);

        result.Should().Contain("List`1");
        result.Should().Contain("first");
        result.Should().Contain("List`1 circular dependency");
    }

}