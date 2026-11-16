using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    private static Person CreatePerson() => new()
    {
        Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
        Name = "Alex",
        Age = 19,
        Height = 180.5,
        BirthDate = new DateTime(2000, 12, 31),
        Address = new Address { City = "Moscow", Street = "Tverskaya", ZipCode = 101000 },
        Hobbies = new List<string> { "Reading", "Coding" },
        Scores = new Dictionary<string, int> { { "Math", 5 }, { "Physics", 4 } }
    };

    private static FieldContainer CreateFieldContainer() => new()
    {
        PublicField = "SomeText",
        IntField = 42,
        GuidField = Guid.Parse("12345678-1234-1234-1234-123456789012")
    };

    [Test]
    public void Serialize_Nested_Objects_ShouldPrintInnerProperties()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>().PrintToString(person);

        Assert.That(result, Does.Contain("Address"));
        Assert.That(result, Does.Contain("City = Moscow"));
        Assert.That(result, Does.Contain("Street = Tverskaya"));
    }

    [Test]
    public void Serialize_Null_Property_ShouldPrintNull()
    {
        var person = CreatePerson();
        person.Address = null;
        var result = ObjectPrinter.For<Person>().PrintToString(person);

        Assert.That(result, Does.Contain("Address = null"));
    }

    [Test]
    public void Serialize_Primitive_As_Root_ShouldPrintValue()
    {
        var result = ObjectPrinter.For<int>().PrintToString(42);
        Assert.That(result.Trim(), Is.EqualTo("42"));
    }

    [Test]
    public void Exclude_Type_ShouldNotSerialize_Anywhere()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>().Exclude<Guid>().PrintToString(person);

        Assert.That(result, Does.Not.Contain("Id"));
        Assert.That(result, Does.Not.Contain("12345678-1234-1234-1234-123456789012"));
    }

    [Test]
    public void Exclude_Type_ShouldNotSerialize_NestedObjects()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>().Exclude<string>().PrintToString(person);

        Assert.That(result, Does.Not.Contain("Name"));
        Assert.That(result, Does.Not.Contain("City"));
        Assert.That(result, Does.Not.Contain("Street"));
    }

    [Test]
    public void Exclude_Type_ShouldWork_With_Public_Fields()
    {
        var container = CreateFieldContainer();
        var result = ObjectPrinter.For<FieldContainer>().Exclude<string>().Exclude<int>().PrintToString(container);

        Assert.That(result, Does.Not.Contain("PublicField"));
        Assert.That(result, Does.Not.Contain("IntField"));
        Assert.That(result, Does.Contain("GuidField"));
    }

    [Test]
    public void Exclude_Property_ShouldNotSerialize()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>().Property(p => p.Name).Exclude().PrintToString(person);

        Assert.That(result, Does.Not.Contain("Name"));
        Assert.That(result, Does.Not.Contain("Alex"));
        Assert.That(result, Does.Contain("City = Moscow"));
    }

    [Test]
    public void Exclude_Field_ShouldNotSerialize()
    {
        var container = CreateFieldContainer();
        var result = ObjectPrinter.For<FieldContainer>()
            .Property(nameof(FieldContainer.PublicField)).Exclude()
            .PrintToString(container);

        Assert.That(result, Does.Not.Contain("PublicField"));
        Assert.That(result, Does.Contain("IntField"));
    }

    [Test]
    public void Using_Type_ShouldOverride_Default_Serialization()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>().Using<int>(x => $"INT_{x}").PrintToString(person);

        Assert.That(result, Does.Contain("INT_19"));
        Assert.That(result, Does.Contain("INT_101000"));
    }

    [TestCase(nameof(Person.Name), "CUSTOM", "Alex")]
    [TestCase(nameof(FieldContainer.PublicField), "CUSTOM", "SomeText")]
    public void Using_Property_ShouldOverride_Default_Serialization(string propertyName, string expected, string notExpected)
    {
        var person = CreatePerson();
        var container = CreateFieldContainer();

        if (propertyName == nameof(Person.Name))
        {
            var result = ObjectPrinter.For<Person>()
                .Property(nameof(Person.Name)).Using(x => expected)
                .PrintToString(person);
            Assert.That(result, Does.Contain(expected));
            Assert.That(result, Does.Not.Contain(notExpected));
        }
        else
        {
            var result = ObjectPrinter.For<FieldContainer>()
                .Property(nameof(FieldContainer.PublicField)).Using(x => expected)
                .PrintToString(container);
            Assert.That(result, Does.Contain(expected));
            Assert.That(result, Does.Not.Contain(notExpected));
        }
    }

    [Test]
    public void Using_Property_ShouldOverride_Using_Type()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>()
            .Using<int>(x => $"INT_{x}")
            .Property(p => p.Age).Using(x => $"AGE_{x}")
            .PrintToString(person);

        Assert.That(result, Does.Contain("AGE_19"));
        Assert.That(result, Does.Contain("INT_101000"));
    }

    [Test]
    public void Using_Nested_Object_ShouldNotPrintInnerProperties()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>()
            .Property(p => p.Address).Using(obj => "ADDRESS_HIDDEN")
            .PrintToString(person);

        Assert.That(result, Does.Contain("Address = ADDRESS_HIDDEN"));
        Assert.That(result, Does.Not.Contain("City = Moscow"));
    }

    [TestCase("ru-RU", "180,5")]
    [TestCase("en-US", "180.5")]
    [TestCase("fr-FR", "180,5")]
    public void UsingCulture_ShouldFormatNumbers(string cultureName, string expectedResult)
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>()
            .UsingCulture<double>(new CultureInfo(cultureName))
            .PrintToString(person);

        Assert.That(result, Does.Contain(expectedResult));
    }

    [TestCase("ru-RU", "31.12.2000")]
    [TestCase("en-US", "12/31/2000")]
    public void UsingCulture_ShouldFormatDateTime(string cultureName, string expectedResult)
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>()
            .UsingCulture<DateTime>(new CultureInfo(cultureName))
            .PrintToString(person);

        Assert.That(result, Does.Contain(expectedResult));
    }

    [Test]
    public void Default_Formatting_Uses_Invariant_Culture()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>().PrintToString(person);

        Assert.That(result, Does.Contain("180.5"));
        Assert.That(result, Does.Not.Contain("180,5"));
    }

    [TestCase(2, "Al", "Alex")]
    [TestCase(10, "Alex", null)]
    [TestCase(0, "", "Alex")]
    public void Trim_String_ShouldTruncate(int length, string expected, string notExpected)
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>().Property(p => p.Name).TrimToLength(length).PrintToString(person);

        Assert.That(result, Does.Contain(expected));
        if (notExpected != null)
            Assert.That(result, Does.Not.Contain(notExpected));
    }

    [Test]
    public void Trim_String_ShouldNotCrashOnNull()
    {
        var person = CreatePerson();
        person.Name = null;
        var result = ObjectPrinter.For<Person>().Property(p => p.Name).TrimToLength(2).PrintToString(person);

        Assert.That(result, Does.Contain("Name = null"));
    }

    [Test]
    public void Trim_String_Collection_ShouldTruncateElements()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>().Property(p => p.Hobbies).TrimToLength(3).PrintToString(person);

        Assert.That(result, Does.Contain("Rea"));
        Assert.That(result, Does.Not.Contain("Reading"));
    }

    [Test]
    public void Circular_Reference_Simple_TwoObjects()
    {
        var parent = new Node { Name = "Parent" };
        var child = new Node { Name = "Child", Parent = parent };
        parent.Parent = child;

        var result = ObjectPrinter.For<Node>().PrintToString(parent);
        Assert.That(result, Does.Contain("<Circular Reference>"));
    }

    [Test]
    public void Circular_Reference_Self_Reference()
    {
        var obj = new SelfReferencing { Name = "Self" };
        obj.Myself = obj;

        var result = ObjectPrinter.For<SelfReferencing>().PrintToString(obj);
        Assert.That(result, Does.Contain("<Circular Reference>"));
    }

    [Test]
    public void Circular_Reference_ThreeObjects()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };
        a.Parent = b; b.Parent = c; c.Parent = a;

        var result = ObjectPrinter.For<Node>().PrintToString(a);
        Assert.That(result, Does.Contain("<Circular Reference>"));
    }

    [Test]
    public void Circular_Reference_In_Collection()
    {
        var parent = new Node { Name = "Parent" };
        var child = new Node { Name = "Child", Parent = parent };
        parent.Children.Add(child);
        child.Children.Add(parent);

        var result = ObjectPrinter.For<Node>().PrintToString(parent);
        Assert.That(result, Does.Contain("<Circular Reference>"));
    }

    [Test]
    public void Duplicate_References_Should_Not_Be_Treated_As_Cycle()
    {
        var shared = new Node { Name = "Shared" };
        var list = new List<Node>
        {
            new() { Name = "Parent1", Parent = shared },
            new() { Name = "Parent2", Parent = shared }
        };

        var result = ObjectPrinter.For<List<Node>>().PrintToString(list);

        Assert.That(result, Does.Not.Contain("<Circular Reference>"));
        var count = Regex.Matches(result, "Shared").Count;
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void Serialize_Collections_List()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>()
            .Exclude<Address>().Exclude<Dictionary<string, int>>()
            .PrintToString(person);

        Assert.That(result, Does.Contain("Hobbies"));
        Assert.That(result, Does.Contain("Reading"));
        Assert.That(result, Does.Contain("Coding"));
    }

    [Test]
    public void Serialize_Collections_Dictionary()
    {
        var person = CreatePerson();
        var result = ObjectPrinter.For<Person>()
            .Exclude<Address>().Exclude<List<string>>()
            .PrintToString(person);

        Assert.That(result, Does.Contain("Scores"));
        Assert.That(result, Does.Contain("Key = Math"));
        Assert.That(result, Does.Contain("Value = 5"));
    }

    [Test]
    public void Extension_Method_Default()
    {
        var person = CreatePerson();
        var result = person.PrintToString();
        Assert.That(result, Does.Contain("Alex"));
        Assert.That(result, Does.Contain("Moscow"));
    }

    [Test]
    public void Extension_Method_With_Config()
    {
        var person = CreatePerson();
        var result = person.PrintToString(config =>
            config.Exclude<Guid>().Property(p => p.Name).TrimToLength(1));

        Assert.That(result, Does.Not.Contain("Id"));
        Assert.That(result, Does.Contain("A"));
        Assert.That(result, Does.Not.Contain("Alex"));
    }
}