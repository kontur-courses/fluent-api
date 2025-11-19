using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class EdgeCasesTests : TestBase
{
    [Test]
    public void PrintToString_WithCyclicReference_HandlesCyclicReference()
    {
        var result = PersonWithCyclicReference.PrintToString();

        result.Should().Contain("Cyclic reference detected");
        Action action = () => PersonWithCyclicReference.PrintToString();
        action.Should().NotThrow();
    }

    [Test]
    public void PrintToString_WithCyclicReferencesInCollections_DetectsCycles()
    {
        var list1 = new List<object>();
        var list2 = new List<object> { list1 };
        list1.Add(list2);

        var result = list1.PrintToString();
        result.Should().Contain("Cyclic reference detected");
    }


    [Test]
    public void PrintToString_WithZeroTrimLength_HandlesCorrectly()
    {
        var testClass = TestPerson;
        var printer = ObjectPrinter.For<Person>()
            .SerializeProperty(x => x.Name).TrimTo(0);

        var result = printer.PrintToString(testClass);
        result.Should().Contain("Name = ");
    }

    [Test]
    public void PrintToString_WithComplexObjectGraph_DoesNotThrow()
    {
        var complexObject = new
        {
            Id = 1,
            Name = "Test",
            Items = new[] { "A", "B", "C" },
            Nested = new { Value = 42, Date = DateTime.Now }
        };

        Action action = () => complexObject.PrintToString();
        action.Should().NotThrow();
    }

    [Test]
    public void PrintToString_WithFailingCustomSerializer_FallsBackToDefault()
    {
        var printer = ObjectPrinter.For<Person>()
            .SerializeType<string>()
            .Use(s => throw new Exception("Test exception"));

        var person = new Person { Name = "Test" };
        Action action = () => printer.PrintToString(person);
        action.Should().NotThrow();
    }

    [Test]
    public void PrintToString_WithSelfReference_DetectsCycle()
    {
        var selfReferencing = new Person();
        selfReferencing.Children.Add(selfReferencing);

        var result = selfReferencing.PrintToString();
        result.Should().Contain("Cyclic reference detected");
    }
    
    [Test]
    public void PrintToString_WithNegativeNestingLevel_ThrowsArgumentException()
    {
        var person = new Person { Name = "Test" };
    
        Action action = () => person.PrintToString(nestingLevel: -1);
    
        action.Should().Throw<ArgumentException>()
            .WithMessage("Nesting level cannot be negative*");
    }
}