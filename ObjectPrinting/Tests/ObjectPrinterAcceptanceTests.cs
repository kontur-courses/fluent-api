
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ObjectPrinting.Configuration;

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
        Address = new Address
        {
            City = "Moscow",
            Street = "Tverskaya",
            ZipCode = 101000
        },
        Hobbies = new List<string> { "Reading", "Coding" },
        Scores = new Dictionary<string, int>
        {
            { "Math", 5 },
            { "Physics", 4 }
        },
        ArrayOfInts = new[] { 1, 2, 3 },
        ArrayOfStrings = new[] { "One", "Two", "Three" }
    };

    private static FieldContainer CreateFieldContainer() => new()
    {
        PublicField = "SomeText",
        IntField = 42,
        GuidField = Guid.Parse("12345678-1234-1234-1234-123456789012")
    };

    [Test]
    public void Serialize_NestedObjects_PrintsInnerProperties()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>().PrintToString(person);

        Assert.That(result, Does.Contain("Address"));
        Assert.That(result, Does.Contain("City = Moscow"));
        Assert.That(result, Does.Contain("Street = Tverskaya"));
    }

    [Test]
    public void Serialize_NullProperty_PrintsNull()
    {
        var person = CreatePerson();
        person.Address = null;

        var result = ObjectPrinter.For<Person>().PrintToString(person);

        Assert.That(result, Does.Contain("Address = null"));
    }

    [Test]
    public void Serialize_PrimitiveAsRoot_PrintsValue()
    {
        var result = ObjectPrinter.For<int>().PrintToString(42);

        Assert.That(result.Trim(), Is.EqualTo("42"));
    }

    [Test]
    public void Serialize_Guid_PrintsValue()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>().PrintToString(person);

        Assert.That(result, Does.Contain("12345678-1234-1234-1234-123456789012"));
        Assert.That(result, Does.Not.Contain("Id = Guid"));
    }

    [Test]
    public void Serialize_ObjectWithIndexer_DoesNotThrowException()
    {
        var obj = new ClassWithIndexer { Name = "Test" };

        var result = ObjectPrinter.For<ClassWithIndexer>().PrintToString(obj);

        Assert.That(result, Does.Contain("Name = Test"));
        Assert.That(result, Does.Not.Contain("Item"));
    }

    [Test]
    public void Serialize_StringWithTrailingSpaces_PreservesSpaces()
    {
        var person = CreatePerson();
        person.Name = "Alex   ";

        var result = ObjectPrinter.For<Person>().PrintToString(person);

        Assert.That(result, Does.Contain("Name = Alex   "));
    }

    [Test]
    public void Exclude_Type_ExcludesEverywhere()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .PrintToString(person);

        Assert.That(result, Does.Not.Contain("Id"));
        Assert.That(result, Does.Not.Contain("12345678-1234-1234-1234-123456789012"));
    }

    [Test]
    public void Exclude_Type_ExcludesNestedProperties()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Exclude<string>()
            .PrintToString(person);

        Assert.That(result, Does.Not.Contain("Name"));
        Assert.That(result, Does.Not.Contain("City"));
        Assert.That(result, Does.Not.Contain("Street"));
    }

    [Test]
    public void Exclude_Type_ExcludesPublicFields()
    {
        var container = CreateFieldContainer();

        var result = ObjectPrinter.For<FieldContainer>()
            .Exclude<string>()
            .Exclude<int>()
            .PrintToString(container);

        Assert.That(result, Does.Not.Contain("PublicField"));
        Assert.That(result, Does.Not.Contain("IntField"));
        Assert.That(result, Does.Contain("GuidField"));
    }

    [Test]
    public void Exclude_Property_ExcludesOnlySpecifiedPath()
    {
        var person = CreatePerson();
        person.Address.Name = "NestedName";

        var result = ObjectPrinter.For<Person>()
            .Property(p => p.Name).Exclude()
            .PrintToString(person);

        Assert.That(result, Does.Not.Contain("Name = Alex"));
        Assert.That(result, Does.Not.Contain("Alex"));
        Assert.That(result, Does.Contain("Name = NestedName"));
        Assert.That(result, Does.Contain("City = Moscow"));
    }

    [Test]
    public void Exclude_Field_ExcludesSpecifiedField()
    {
        var container = CreateFieldContainer();

        var result = ObjectPrinter.For<FieldContainer>()
            .Property(nameof(FieldContainer.PublicField)).Exclude()
            .PrintToString(container);

        Assert.That(result, Does.Not.Contain("PublicField"));
        Assert.That(result, Does.Contain("IntField"));
    }

    [Test]
    public void Exclude_Property_OverridesUsingSerializer()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Property(p => p.Name).Using(x => "CUSTOM_NAME")
            .Property(p => p.Name).Exclude()
            .PrintToString(person);

        Assert.That(result, Does.Not.Contain("CUSTOM_NAME"));
        Assert.That(result, Does.Not.Contain("Name = Alex"));
        Assert.That(result, Does.Contain("Name = null"));
    }

    [Test]
    public void Exclude_Type_OverridesUsingSerializer()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Using<string>(x => "CUSTOM_STRING")
            .Exclude<string>()
            .PrintToString(person);

        Assert.That(result, Does.Not.Contain("CUSTOM_STRING"));
        Assert.That(result, Does.Not.Contain("Alex"));
    }

    [Test]
    public void Using_Type_OverridesDefaultSerialization()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Using<int>(x => $"INT_{x}")
            .PrintToString(person);

        Assert.That(result, Does.Contain("INT_19"));
        Assert.That(result, Does.Contain("INT_101000"));
    }

    [TestCase(nameof(Person.Name), "CUSTOM", "Alex")]
    [TestCase(nameof(FieldContainer.PublicField), "CUSTOM", "SomeText")]
    public void Using_Property_OverridesDefaultSerialization(
        string propertyName,
        string expected,
        string notExpected)
    {
        if (propertyName == nameof(Person.Name))
        {
            var person = CreatePerson();

            var result = ObjectPrinter.For<Person>()
                .Property(nameof(Person.Name)).Using(x => expected)
                .PrintToString(person);

            Assert.That(result, Does.Contain(expected));
            Assert.That(result, Does.Not.Contain(notExpected));
        }
        else
        {
            var container = CreateFieldContainer();

            var result = ObjectPrinter.For<FieldContainer>()
                .Property(nameof(FieldContainer.PublicField)).Using(x => expected)
                .PrintToString(container);

            Assert.That(result, Does.Contain(expected));
            Assert.That(result, Does.Not.Contain(notExpected));
        }
    }

    [Test]
    public void Using_Property_OverridesTypeSerializer()
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
    public void Using_NestedObject_DoesNotPrintInnerProperties()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Property(p => p.Address).Using(obj => "ADDRESS_HIDDEN")
            .PrintToString(person);

        Assert.That(result, Does.Contain("Address = ADDRESS_HIDDEN"));
        Assert.That(result, Does.Not.Contain("City = Moscow"));
    }

    [Test]
    public void Using_Property_OverridesTrimSetting()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Property(p => p.Name).TrimToLength(2)
            .Property(p => p.Name).Using(x => "FULL_NAME")
            .PrintToString(person);

        Assert.That(result, Does.Contain("FULL_NAME"));
        Assert.That(result, Does.Not.Contain("Al"));
    }

    [Test]
    public void Property_Overloads_ReturnCorrectConfigurationTypes()
    {
        var stringConfig = ObjectPrinter.For<Person>().Property(p => p.Name);
        var intConfig = ObjectPrinter.For<Person>().Property(p => p.Age);

        Assert.That(stringConfig, Is.InstanceOf<IStringPropertyConfig<Person>>());
        Assert.That(intConfig, Is.InstanceOf<IPropertyConfig<Person>>());
        Assert.That(intConfig, Is.Not.InstanceOf<IStringPropertyConfig<Person>>());
    }

    [TestCase("ru-RU", "180,5")]
    [TestCase("en-US", "180.5")]
    [TestCase("fr-FR", "180,5")]
    public void UsingCulture_NumbersAreFormattedCorrectly(
        string cultureName,
        string expectedResult)
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .UsingCulture<double>(new CultureInfo(cultureName))
            .PrintToString(person);

        Assert.That(result, Does.Contain(expectedResult));
    }

    [TestCase("ru-RU", "31.12.2000")]
    [TestCase("en-US", "12/31/2000")]
    public void UsingCulture_DateTimeIsFormattedCorrectly(
        string cultureName,
        string expectedResult)
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .UsingCulture<DateTime>(new CultureInfo(cultureName))
            .PrintToString(person);

        Assert.That(result, Does.Contain(expectedResult));
    }

    [Test]
    public void DefaultFormatting_NumbersUseInvariantCulture()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>().PrintToString(person);

        Assert.That(result, Does.Contain("180.5"));
        Assert.That(result, Does.Not.Contain("180,5"));
    }

    [TestCase(2, "Al", "Alex")]
    [TestCase(10, "Alex", null)]
    [TestCase(0, "", "Alex")]
    public void Trim_String_TruncatesToSpecifiedLength(
        int length,
        string expected,
        string notExpected)
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Property(p => p.Name).TrimToLength(length)
            .PrintToString(person);

        Assert.That(result, Does.Contain(expected));

        if (notExpected != null)
            Assert.That(result, Does.Not.Contain(notExpected));
    }

    [Test]
    public void Trim_NullString_DoesNotThrowException()
    {
        var person = CreatePerson();
        person.Name = null;

        var result = ObjectPrinter.For<Person>()
            .Property(p => p.Name).TrimToLength(2)
            .PrintToString(person);

        Assert.That(result, Does.Contain("Name = null"));
    }

    [Test]
    public void Trim_NegativeLength_ThrowsArgumentOutOfRangeException()
    {
        var printer = ObjectPrinter.For<Person>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            printer.Property(p => p.Name).TrimToLength(-1));
    }

    [Test]
    public void Property_WithNestedPath_AppliesOnlyToSpecifiedPath()
    {
        var person = CreatePerson();
        person.Address.Name = "NestedName";

        var result = ObjectPrinter.For<Person>()
            .Property(p => p.Address.Name).Exclude()
            .PrintToString(person);

        Assert.That(result, Does.Not.Contain("NestedName"));

        Assert.That(result, Does.Contain("Name = Alex"));
        Assert.That(result, Does.Contain("City = Moscow"));
    }

    [Test]
    public void Serialize_CollectionsList_PrintsElements()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Exclude<Address>()
            .Exclude<Dictionary<string, int>>()
            .PrintToString(person);

        Assert.That(result, Does.Contain("Hobbies"));
        Assert.That(result, Does.Contain("Reading"));
        Assert.That(result, Does.Contain("Coding"));
    }

    [Test]
    public void Serialize_CollectionsDictionary_PrintsKeyValuePairs()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Exclude<Address>()
            .Exclude<List<string>>()
            .PrintToString(person);

        Assert.That(result, Does.Contain("Scores"));
        Assert.That(result, Does.Contain("Key = Math"));
        Assert.That(result, Does.Contain("Value = 5"));
    }

    [Test]
    public void Serialize_ArrayOfInts_PrintsElements()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Exclude<Address>()
            .Exclude<Dictionary<string, int>>()
            .Exclude<List<string>>()
            .Exclude<string[]>()
            .PrintToString(person);

        Assert.That(result, Does.Contain("ArrayOfInts"));
        Assert.That(result, Does.Contain("1"));
        Assert.That(result, Does.Contain("2"));
        Assert.That(result, Does.Contain("3"));
    }

    [Test]
    public void Serialize_ArrayOfStrings_PrintsElements()
    {
        var person = CreatePerson();

        var result = ObjectPrinter.For<Person>()
            .Exclude<Address>()
            .Exclude<Dictionary<string, int>>()
            .Exclude<List<string>>()
            .Exclude<int[]>()
            .PrintToString(person);

        Assert.That(result, Does.Contain("ArrayOfStrings"));
        Assert.That(result, Does.Contain("One"));
        Assert.That(result, Does.Contain("Two"));
        Assert.That(result, Does.Contain("Three"));
    }

    [Test]
    public void Serialize_ArrayAsRoot_PrintsElements()
    {
        var array = new[] { 10, 20, 30 };

        var result = ObjectPrinter.For<int[]>().PrintToString(array);

        Assert.That(result, Does.Contain("Int32[]"));
        Assert.That(result, Does.Contain("10"));
        Assert.That(result, Does.Contain("20"));
        Assert.That(result, Does.Contain("30"));
    }

    [Test]
    public void Serialize_MultidimensionalArray_DoesNotThrowException()
    {
        var array = new int[,] { { 1, 2 }, { 3, 4 } };

        var result = ObjectPrinter.For<int[,]>().PrintToString(array);

        Assert.That(result, Does.Contain("Int32[,]"));
    }

    [Test]
    public void Serialize_SelfReference_DetectsCycle()
    {
        var obj = new SelfReferencing { Name = "Self" };
        obj.Myself = obj;

        var result = ObjectPrinter.For<SelfReferencing>().PrintToString(obj);

        Assert.That(result, Does.Contain("<Circular Reference>"));
    }

    [Test]
    public void Serialize_CircularReferenceBetweenTwoObjects_DetectsCycle()
    {
        var parent = new Node { Name = "Parent" };
        var child = new Node { Name = "Child", Parent = parent };
        parent.Parent = child;

        var result = ObjectPrinter.For<Node>().PrintToString(parent);

        Assert.That(result, Does.Contain("<Circular Reference>"));
    }

    [Test]
    public void Serialize_ThreeObjectsWithCircularReferences_DetectsCycle()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };

        a.Parent = b;
        b.Parent = c;
        c.Parent = a;

        var result = ObjectPrinter.For<Node>().PrintToString(a);

        Assert.That(result, Does.Contain("<Circular Reference>"));
    }

    [Test]
    public void Serialize_CircularReferenceInCollection_DetectsCycle()
    {
        var parent = new Node { Name = "Parent" };
        var child = new Node { Name = "Child", Parent = parent };

        parent.Children.Add(child);
        child.Children.Add(parent);

        var result = ObjectPrinter.For<Node>().PrintToString(parent);

        Assert.That(result, Does.Contain("<Circular Reference>"));
    }

    [Test]
    public void Serialize_DuplicateReferences_DoesNotDetectCycle()
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
    public void Serialize_DifferentObjectsWithSameValues_DoesNotDetectCycle()
    {
        var shared1 = new Node { Name = "Shared" };
        var shared2 = new Node { Name = "Shared" };

        var list = new List<Node> { shared1, shared2 };

        var result = ObjectPrinter.For<List<Node>>().PrintToString(list);

        Assert.That(result, Does.Not.Contain("<Circular Reference>"));

        var count = Regex.Matches(result, "Shared").Count;
        Assert.That(count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void ExtensionMethod_DefaultConfiguration_PrintsObject()
    {
        var person = CreatePerson();

        var result = person.PrintToString();

        Assert.That(result, Does.Contain("Alex"));
        Assert.That(result, Does.Contain("Moscow"));
    }

    [Test]
    public void ExtensionMethod_CustomConfiguration_AppliesSettings()
    {
        var person = CreatePerson();

        var result = person.PrintToString(config =>
            config.Exclude<Guid>().Property(p => p.Name).TrimToLength(1));

        Assert.That(result, Does.Not.Contain("Id"));
        Assert.That(result, Does.Contain("A"));
        Assert.That(result, Does.Not.Contain("Alex"));
    }
}
