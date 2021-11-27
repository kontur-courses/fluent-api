using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

public class ObjectPrintingTests
{
    private string newLine;
    private Person person;
    private PersonWithParent personWithCyclicReference;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        person = new Person { Name = "Alex", Age = 19, Height = 170.1, Id = new Guid("c76d3d06-3ab9-4d55-85e7-9f62d4f49466") };
        personWithCyclicReference = new PersonWithParent { Name = "Alex", Parent = new PersonWithParent { Name = "Notalex" } };
        personWithCyclicReference.Parent.Parent = personWithCyclicReference;

        newLine = Environment.NewLine;
    }

    [Test]
    public void AcceptanceTest()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding<Guid>()
            .Printing<int>().Using(i => i.ToString("X"))
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            .Printing(x => x.Height).Using(i => i.ToString("P"))
            .Printing(p => p.Name).TrimmedToLength(10)
            .Excluding(p => p.Age)
            .Including<DateTime>()
            .Including(x => x.Age);

        var s1 = printer.PrintToString(person);
        var s2 = person.PrintToString();
        var s3 = person.PrintToString(s => s.Excluding(p => p.Age).Excluding(p => p.Name));

        Console.WriteLine(s1);
        Console.WriteLine(s2);
        Console.WriteLine(s3);
    }

    [Test]
    public void ShouldSerializeFully_WhenSerializationParametersAreDefault()
    {
        var actual = person.PrintToString();

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alex{newLine}\tHeight: 170,1{newLine}\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldExcludeMember_WhenItsTypeExcluded()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding<int>();

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alex{newLine}\tHeight: 170,1{newLine}");
    }

    [Test]
    public void ShouldExcludeMember_WhenItsPathExcluded()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding(p => p.Name);

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tHeight: 170,1{newLine}\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldIncludeMember_WhenItsTypeExcludingOverriden()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding<int>()
            .Including<int>();

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alex{newLine}\tHeight: 170,1{newLine}\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldIncludeMember_WhenItsPathExcludingOverriden()
    {
        var printer = ObjectPrinter.For<Person>()
            .Excluding(p => p.Name)
            .Including(p => p.Name);

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alex{newLine}\tHeight: 170,1{newLine}\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldUseCustomTypeSerializer_WhenSpecifiedSerializerForType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing<int>()
            .Using(x => x.ToString("X"));

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alex{newLine}\tHeight: 170,1{newLine}\tAge: 13{newLine}");
    }

    [Test]
    public void ShouldUseCustomMemberSerializer_WhenSpecifiedSerializerForMember()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing(x => x.Age)
            .Using(x => x.ToString("X"));

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alex{newLine}\tHeight: 170,1{newLine}\tAge: 13{newLine}");
    }

    [Test]
    public void ShouldUseCustomTypeCulture_WhenSpecifiedCultureForType()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing<double>()
            .Using(CultureInfo.GetCultureInfo("en-US"));

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alex{newLine}\tHeight: 170.1{newLine}\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldUseCustomMemberCulture_WhenSpecifiedCultureForMember()
    {
        var printer = ObjectPrinter.For<Person>()
            .Printing(x => x.Height)
            .Using(CultureInfo.GetCultureInfo("en-US"));

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alex{newLine}\tHeight: 170.1{newLine}\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldUseTrimming_WhenItsSpecifiedForType()
    {
        var person = new Person { Name = "Alexalexalex", Age = 19, Height = 170.1, Id = new Guid("c76d3d06-3ab9-4d55-85e7-9f62d4f49466") };

        var printer = ObjectPrinter.For<Person>()
            .Printing<string>()
            .TrimmedToLength(10);

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alexalexal{newLine}\tHeight: 170,1{newLine}\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldUseTrimming_WhenItsSpecifiedForMember()
    {
        var person = new Person { Name = "Alexalexalex", Age = 19, Height = 170.1, Id = new Guid("c76d3d06-3ab9-4d55-85e7-9f62d4f49466") };

        var printer = ObjectPrinter.For<Person>()
            .Printing(x => x.Name)
            .TrimmedToLength(10);

        var actual = printer.PrintToString(person);

        actual.Should().Be($"Person:{newLine}\tId: c76d3d06-3ab9-4d55-85e7-9f62d4f49466{newLine}\tName: Alexalexal{newLine}\tHeight: 170,1{newLine}\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldThrowOnTrimmedToLength_WhenThereIsNegativeMaxLength()
    {
        var action = () => ObjectPrinter.For<Person>()
            .Printing(x => x.Name)
            .TrimmedToLength(-1); ;

        action.Should().Throw<ArgumentException>().WithMessage("Maximum length cannot be negative");
    }

    [Test]
    public void ShouldThrowOnCyclicReference_WhenOtherBehaviorIsNotSpecified()
    {
        var action = () => personWithCyclicReference.PrintToString();

        action.Should().Throw<InvalidOperationException>("Cyclic reference detected");
    }

    [Test]
    public void ShouldThrowOnCyclicReference_WhenObjectReferencesOnItself()
    {
        var objectWithReferenceOnItself = new PersonWithParent();
        objectWithReferenceOnItself.Parent = objectWithReferenceOnItself;
        
        var action = () => personWithCyclicReference.PrintToString();

        action.Should().Throw<InvalidOperationException>("Cyclic reference detected");
    }

    [Test]
    public void ShouldUseCyclicReferenceMessage_WhenItsSpecified()
    {
        var printer = ObjectPrinter.For<PersonWithParent>()
            .UseCyclicReferenceMessage("Cyclic reference");

        var actual = printer.PrintToString(personWithCyclicReference);

        actual.Should().Be($"PersonWithParent:{newLine}\tName: Alex{newLine}\tParent: PersonWithParent:{newLine}\t\tName: Notalex{newLine}\t\tParent: Cyclic reference{newLine}");
    }

    [Test]
    public void ShouldThrowOnCyclicReference_WhenCyclicReferenceMessageOverridenByThrowing()
    {
        var printer = ObjectPrinter.For<PersonWithParent>()
            .UseCyclicReferenceMessage("Cyclic reference")
            .ThrowOnCyclicReference();

        var action = () => printer.PrintToString(personWithCyclicReference);

        action.Should().Throw<InvalidOperationException>("Cyclic reference detected");
    }

    [Test]
    public void ShouldSerializeCorrectly_WhenSerializedObjectContainsChildObjectRepeatedly()
    {
        var obj = new LinkedList<object>();
        var innerObj = new LinkedList<int>();
        innerObj.AddLast(1);
        obj.AddLast(innerObj);
        obj.AddLast(innerObj);

        var actual = obj.PrintToString();
        
        actual.Should().Be($":{newLine}\t- :{newLine}\t\t- 1{newLine}\t- :{newLine}\t\t- 1{newLine}");
    }

    [Test]
    public void ShouldSerializeCorrectly_WhenArgumentIsCollection()
    {
        var list = new List<int> { 1, 2, 3 };

        var actual = list.PrintToString();

        actual.Should().Be($":{newLine}\t- 1{newLine}\t- 2{newLine}\t- 3{newLine}");
    }

    [Test]
    public void ShouldSerializeCorrectly_WhenArgumentIsCollectionWithObject()
    {
        var list = new List<Person> { person, person };
        var printer = ObjectPrinter.For<List<Person>>()
            .Excluding<double>()
            .Excluding<Guid>();

        var actual = printer.PrintToString(list);

        actual.Should().Be($":{newLine}\t- Person:{newLine}\t\tName: Alex{newLine}\t\tAge: 19{newLine}\t- Person:{newLine}\t\tName: Alex{newLine}\t\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldSerializeCorrectly_WhenArgumentIsEmptyCollection()
    {
        var list = new List<int>();

        var actual = list.PrintToString();

        actual.Should().Be($":{newLine}");
    }

    [Test]
    public void ShouldSerializeCorrectly_WhenArgumentIsDictionary()
    {
        var dictionary = new Dictionary<string, int> { { "one", 1 }, { "two", 2 }, { "three", 3 } };

        var actual = dictionary.PrintToString();

        actual.Should().Be($":{newLine}\t- key: one{newLine}\t  value: 1{newLine}\t- key: two{newLine}\t  value: 2{newLine}\t- key: three{newLine}\t  value: 3{newLine}");
    }

    [Test]
    public void ShouldSerializeCorrectly_WhenArgumentIsDictionaryWithObjects()
    {
        var dictionary = new Dictionary<Person, Person> { { person, person }};
        var printer = ObjectPrinter.For<Dictionary<Person, Person>>()
            .Excluding<double>()
            .Excluding<Guid>();

        var actual = printer.PrintToString(dictionary);

        actual.Should().Be($":{newLine}\t- key: Person:{newLine}\t\tName: Alex{newLine}\t\tAge: 19{newLine}\t  value: Person:{newLine}\t\tName: Alex{newLine}\t\tAge: 19{newLine}");
    }

    [Test]
    public void ShouldSerializeCorrectly_WhenArgumentIsEmptyDictionary()
    {
        var dictionary = new Dictionary<string, int>();

        var actual = dictionary.PrintToString();

        actual.Should().Be($":{newLine}");
    }
}