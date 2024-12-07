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
            Age = 10
        };
    }

    [Test]
    public Task ObjectPrinter_ExcludeType() => 
        Verify(person.PrintToString(c => c.Exclude<Guid>()));

    [Test]
    public Task ObjectPrinter_ExcludeProperty() =>
        Verify(person.PrintToString(c => c.Exclude(p => p.Id)));

    [Test]
    public Task ObjectPrinter_SerializeType() =>
        Verify(person.PrintToString(c => c.Printing<string>().Using(s => $"\"{s}\"")));

    [Test]
    public Task ObjectPrinter_SerializeProperty() =>
        Verify(person.PrintToString(c => c.Printing(p => p.Name).Using(s => $"\"{s}\"")));

    [Test]
    public Task ObjectPrinter_SetCultureForType() =>
        Verify(person.PrintToString(c => c.Printing<double>().WithCulture(CultureInfo.InvariantCulture)));

    [Test]
    public Task ObjectPrinter_SetCultureForProperty() =>
        Verify(person.PrintToString(c => c.Printing(p => p.Height).WithCulture(CultureInfo.InvariantCulture)));

    [Test]
    public Task ObjectPrinter_CutStringType() =>
        Verify(person.PrintToString(c => c.Printing<string>().Cut(3)));

    [Test]
    public Task ObjectPrinter_CutStringProperty() =>
        Verify(person.PrintToString(c => c.Printing(p => p.Name).Cut(3)));

    [Test]
    public Task ObjectPrinter_FirstCheckPropertyConfigs()
    {
        var obj = person.PrintToString(c => c
                                         .Printing<string>().Using(s => $"\"{s}\"")
                                         .Printing(p => p.Name).Using(s => $"Меня зовут {s}")
                                         .Printing<double>().WithCulture(CultureInfo.InvariantCulture)
                                         .Printing(p => p.Height).WithCulture(CultureInfo.CurrentCulture));

        return Verify(obj);
    }

    [Test]
    public Task ObjectPrinter_PrintSimpleCollections() =>
        Verify((new int[] { 1, 2, 3 }).PrintToString());

    [Test]
    public Task ObjectPrinter_PrintPersonCollections()
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
    public void ObjectPrinter_DontThrowStackOverflowException()
    {
        var person = GenerateBigRecursionPerson();
        var a = person.PrintToString();
        var action = () => person.PrintToString();

        action.Should().NotThrow();
    }


    private static Task Verify(string printingObject) => 
        Verifier.Verify(printingObject, settings);

    private PersonWithChild GenerateBigRecursionPerson()
    {
        var person = new PersonWithChild();
        var recursionSize = 10000;
        var currentPerson = person;

        for (var i = 0; i < recursionSize; i++)
        {
            currentPerson.Child = new PersonWithChild();
            currentPerson = currentPerson.Child;
        }

        return person;
    }
}
