using System;
using System.Collections.Generic;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using NUnit.Framework;
using ObjectPrinting.Extensions;
using ObjectPrinting.Tests.Data;

namespace ObjectPrinting.Tests;

[UseApprovalSubdirectory("Results")]
[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [SetUp]
    public void SetUp()
    {
        person = new Person { Height = 80.05, Name = "John", Age = 50, Surname = "Wick" };
    }

    private Person person;

    [Test]
    public void Demo()
    {
        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Exclude<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .For<string>().Using(s => s.ToUpper())
            //3. Для числовых типов указать культуру
            .For<double>().Using(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            .For(p => p.Age).Using(s => "age: " + s)
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .For(p => p.Name).Trim(2)
            //6. Исключить из сериализации конкретного свойства
            .Exclude(p => p.Surname);

        var s1 = printer.PrintToString(person);
        Console.WriteLine(s1);
        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию   
        Console.WriteLine(person.PrintToString());
        //8. ...с конфигурированием
        Console.WriteLine(person.PrintToString(c => c.Exclude<double>()));
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializePersonCorrectly()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldHandleDirectCyclicReference()
    {
        var node = new Node(1);
        node.Child = node;

        var result = ObjectPrinter.For<Node>()
            .PrintToString(node);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldExcludeMember()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude(p => p.Age)
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldExcludeType()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude<string>()
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldExcludeNothing()
    {
        var result = ObjectPrinter.For<Person>()
            .Exclude<Node>()
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldUseCustomTypeSerializer()
    {
        var result = ObjectPrinter.For<Person>()
            .For<string>()
            .Using(s => s.ToUpper())
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldUseCustomMemberSerializer()
    {
        var result = ObjectPrinter.For<Person>()
            .For(p => p.Age)
            .Using(s => s + " лет")
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldApplyMultipleSerializersToMember()
    {
        var result = ObjectPrinter.For<Person>()
            .For(p => p.Age)
            .Using(s => $"{s} лет")
            .For(p => p.Age)
            .Using(s => $"возраст: {s}")
            .For(p => p.Name)
            .Using(s => s.ToUpper())
            .For(p => p.Name)
            .Trim(2)
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldApplyCultureForType()
    {
        var result = ObjectPrinter.For<Person>()
            .For<double>()
            .Using(CultureInfo.InvariantCulture)
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldApplyCultureForMember()
    {
        var result = ObjectPrinter.For<Person>()
            .For(p => p.Height)
            .Using(CultureInfo.InvariantCulture)
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldTrimStringProperty()
    {
        var result = ObjectPrinter.For<Person>()
            .For(p => p.Name)
            .Trim(2)
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldNotTrimStringIfMaxLengthIsGreaterThanStringLength()
    {
        var result = ObjectPrinter.For<Person>()
            .For(p => p.Name)
            .Trim(10)
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldHandleDeepCyclicReferences()
    {
        var node = new Node(1);
        var child1 = new Node(2);
        var child2 = new Node(3);
        var child3 = new Node(4);
        node.Child = child1;
        child1.Child = child2;
        child2.Child = child3;
        child3.Child = node;

        var result = ObjectPrinter.For<Node>()
            .PrintToString(node);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializeArray()
    {
        var array = new[] { 1, 2, 3 };

        var result = ObjectPrinter.For<int[]>()
            .PrintToString(array);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializeList()
    {
        var list = new List<int> { 3, 4, 5 };

        var result = ObjectPrinter.For<List<int>>()
            .PrintToString(list);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializeEmptyList()
    {
        var list = new List<int>();

        var result = ObjectPrinter.For<List<int>>()
            .PrintToString(list);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializeDictionary()
    {
        var dictionary = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 3 }
        };

        var result = ObjectPrinter.For<Dictionary<int, int>>()
            .PrintToString(dictionary);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializeEmptyDictionary()
    {
        var dictionary = new Dictionary<int, int>();

        var result = ObjectPrinter.For<Dictionary<int, int>>()
            .PrintToString(dictionary);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldHandleCyclicReferencesInCollection()
    {
        var node = new Node(1);
        var child1 = new Node(2);
        node.Child = child1;
        child1.Child = node;
        var list = new List<Node> { node, child1 };

        var result = ObjectPrinter.For<List<Node>>()
            .PrintToString(list);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializeNullProperty()
    {
        person.Name = null;

        var result = ObjectPrinter.For<Person>()
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldPrioritizeMemberSerializerOverTypeSerializer()
    {
        var result = ObjectPrinter.For<Person>()
            .For<string>().Using(s => s.ToUpper())
            .For(p => p.Name).Using(s => s.ToLower())
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldPrioritizeMemberCultureOverTypeCulture()
    {
        var result = ObjectPrinter.For<Person>()
            .For<double>().Using(CultureInfo.InvariantCulture)
            .For(p => p.Height).Using(CultureInfo.CurrentCulture)
            .PrintToString(person);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializeEnumProperty()
    {
        var toggle = new Toggle
        {
            Status = Status.Active
        };

        var result = ObjectPrinter.For<Toggle>()
            .PrintToString(toggle);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializePublicField()
    {
        var tree = new Tree(new Node(1), 15);

        var result = ObjectPrinter.For<Tree>()
            .Exclude<int>()
            .PrintToString(tree);

        Approvals.Verify(result);
    }

    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldSerializePrivateField()
    {
        var tree = new Tree(new Node(1), 15);

        var result = ObjectPrinter.For<Tree>()
            .PrintToString(tree);

        Approvals.Verify(result);
    }
    
    [Test]
    [UseReporter(typeof(DiffReporter))]
    public void PrintToString_ShouldApplyTypeSerializerWhenTrimIsUsedOnProperty()
    {
        var result = ObjectPrinter.For<Person>()
            .For<string>().Using(s => s.ToUpper())
            .For(p => p.Name).Trim(2)
            .PrintToString(person);
        
        Approvals.Verify(result);
    }
}