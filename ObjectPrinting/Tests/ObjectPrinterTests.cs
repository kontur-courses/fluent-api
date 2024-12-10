using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using VerifyNUnit;
using VerifyTests;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterTests
{
    private static readonly VerifySettings settings = new();
    private Person person;

    [SetUp]
    public void SetUp()
    {
        settings.UseDirectory("cases");
        person = new Person()
        {
            Name = "Ivan",
            SecondName = "Ivanov",
            Height = 0.12345,
            Weight = 0.1234,
            Age = 10,
            Birthday = new(2004, 2, 16),
            Nickname = "Vanya12"
        };
    }

    [Test]
    public Task ExcludeType() => 
        Verify(person.PrintToString(c => c.Exclude<string>()));

    [Test]
    public Task ExcludeProperty() =>
        Verify(person.PrintToString(c => c.Exclude(p => p.Id)));

    [Test]
    public Task SerializeType() =>
        Verify(person.PrintToString(c => c.Printing<string>().Using(s => $"\"{s}\"")));

    [Test]
    public Task SerializeProperty() =>
        Verify(person.PrintToString(c => c.Printing(p => p.Name).Using(s => $"\"{s}\"")));

    [Test]
    public Task SetCultureForType() =>
        Verify(person.PrintToString(c => c.Printing<double>().WithCulture(CultureInfo.InvariantCulture)));

    [Test]
    public Task SetCultureForProperty() =>
        Verify(person.PrintToString(c => c.Printing(p => p.Height).WithCulture(CultureInfo.InvariantCulture)));

    [Test]
    public Task CutStringType() =>
        Verify(person.PrintToString(c => c.Printing<string>().Cut(3)));

    [Test]
    public Task DontCutStringTypeIfTextLengthLtThenMaxLen() =>
        Verify(person.PrintToString(p => p.Printing<string>().Cut(1000)));

    [Test]
    public Task CutStringProperty() =>
        Verify(person.PrintToString(c => c.Printing(p => p.Name).Cut(3)));

    [Test]
    public Task FirstCheckPropertyConfigs()
    {
        var obj = person.PrintToString(c => c
                                         .Printing<string>().Using(s => $"\"{s}\"")
                                         .Printing(p => p.Name).Using(s => $"Меня зовут {s}")
                                         .Printing<double>().WithCulture(CultureInfo.InvariantCulture)
                                         .Printing(p => p.Height).WithCulture(CultureInfo.CurrentCulture));

        return Verify(obj);
    }

    [Test]
    public Task PrintSimpleCollections() =>
        Verify((new int[] { 1, 2, 3 }).PrintToString());

    [Test]
    public Task PrintPersonCollections()
    {
        var objs = new List<Person>()
        {
            new() { Name = "Ivan" },
            new() { Name = "Ivan1" },
            new() { Name = "Ivan2" }
        };

        return Verify(objs.PrintToString());
    }

    [Test]
    public Task ProcessesRecursionCorrect()
    {
        person = GenerateRecursionPerson();
        
        return Verify(person.PrintToString());
    }

    [Test]
    public Task SerializeNestedEnumerable()
    {
        var objectWithNestedList = new ObjectWithNestedList([1, 2, 3], "Object");

        return Verify(objectWithNestedList.PrintToString());
    }

    [Test]
    public Task SerializeDictionary()
    {
        var person2 = new Person { Name = "Arcadiy" };
        var dict = new Dictionary<int, Person>
        {
            { 1, person },
            { 2, person2 }
        };

        return Verify(dict.PrintToString());
    }

    [Test]
    public Task SerializeObjectWithNullName() =>
        Verify(10.PrintToString());

    [Test]
    public void DontThrowStackOverflowException()
    {
        var person = GenerateRecursionPerson();
        var action = () => person.PrintToString();

        action.Should().NotThrow<StackOverflowException>();
    }

    [Test]
    public void RecursionDontThrowStackOverflowException()
    {
        var recursionObj = new RecursionObject();
        recursionObj.Object = recursionObj;
        var recursionClass = new Recursion() { Object = recursionObj };
        var a = recursionClass.PrintToString();
        var action = () => recursionClass.PrintToString();

        action.Should().NotThrow<StackOverflowException>();
    }


    private static Task Verify(string printingObject) => 
        Verifier.Verify(printingObject, settings);

    private PersonWithChild GenerateRecursionPerson()
    {
        var person = new PersonWithChild();
        person.Child = person;
        return person;
    }
}

record class ObjectWithNestedList(List<int> Ints, string Name);
