using System.Globalization;
using FluentAssertions;
using ObjectPrinting.Extensions;
using ObjectPrinting.Models;
using ObjectPrinting.PrintingConfiguration;

namespace ObjectPrinting.Tests;

public class ObjectPrinter_Should
{
    private PrintingConfig<Person> personConfig;
    private Person person1;
    private Person person2;
    private Person person3;

    [SetUp]
    public void Setup()
    {
        personConfig = ObjectPrinter.For<Person>();
        person1 = new Person { Age = 29, Height = 150.45, Id = Guid.NewGuid(), Name = "James Hetfield" };
        person2 = new Person { Age = 46, Height = 200.34, Id = Guid.NewGuid(), Name = "Lars Ulrich" };
        person3 = new Person { Age = 50, Height = 183.46, Id = Guid.NewGuid(), Name = "Kirk Hammett" };
    }

    [Test]
    public void PrintToString_ShouldReturnObject_WOConfiguration()
    {
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain($"Age = {person1.Age}");
        configuredPrintToString.Should().Contain($"Name = {person1.Name}");
        configuredPrintToString.Should().Contain($"Id = {person1.Id.ToString()}");
        configuredPrintToString.Should().Contain($"Height = {person1.Height}");
        configuredPrintToString.Should().Contain($"Birthday = {person1.Birthday}");
    }

    [Test]
    public void Exclude_ShouldExclude_Type()
    {
        personConfig.For<int>().Exclude();
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().NotContain("Age");
    }

    [Test]
    public void Exclude_ShouldExclude_ManyTypes()
    {
        personConfig.For<int>().Exclude()
            .For<string>().Exclude()
            .For<Guid>().Exclude();
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().NotContain("Id");
        configuredPrintToString.Should().NotContain("Name");
        configuredPrintToString.Should().NotContain("Age");
    }

    [Test]
    public void Exclude_ShouldExclude_Property()
    {
        personConfig.Exclude(p => p.Id);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().NotContain("Id");
    }

    [Test]
    public void Exclude_ShouldExclude_ManyProperties()
    {
        personConfig.Exclude(p => p.Id)
            .Exclude(p => p.Name);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().NotContain("Id").And.NotContain("Name");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void ChangeSerialization_ShouldCustomizeSerialization_ForProperty()
    {
        personConfig.For(p => p.Height).ChangeSerialization(height => height.ToString("P"));
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain($"Height = {person1.Height:P}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void ChangeSerialization_ShouldCustomizeSerialization_ForType()
    {
        personConfig.For<double>().ChangeSerialization(d => d.ToString("P"));
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain($"Height = {person1.Height:P}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void UseCulture_ShouldCustomizeCulture_ForType()
    {
        personConfig.For<double>().UseCulture(CultureInfo.InvariantCulture);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain($"Height = {person1.Height.ToString(CultureInfo.InvariantCulture)}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void UseCulture_ShouldNotCustomizeCulture_ForDifferentType()
    {
        personConfig.For<double>().UseCulture(CultureInfo.InvariantCulture);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain($"Height = {person1.Height.ToString(CultureInfo.InvariantCulture)}");
        configuredPrintToString.Should().Contain($"Birthday = {person1.Birthday.ToString(CultureInfo.CurrentCulture)}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void UseCulture_ShouldCustomizeCulture_ForManyTypes()
    {
        personConfig.For<double>().UseCulture(CultureInfo.InvariantCulture)
            .For<DateTime>().UseCulture(CultureInfo.InvariantCulture);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain($"Height = {person1.Height.ToString(CultureInfo.InvariantCulture)}");
        configuredPrintToString.Should()
            .Contain($"Birthday = {person1.Birthday.ToString(CultureInfo.InvariantCulture)}");
        Console.WriteLine(configuredPrintToString);
    }

    [TestCase(0)]
    [TestCase(5)]
    public void SetLength_ShouldTrim_SmallStrings(int count)
    {
        personConfig.For(s => s.Name).SetLength(count);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain($"Name = {person1.Name[..count] + Environment.NewLine}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void SetLength_ShouldNotChangeString_IfLengthMoreThanSizeOfAString()
    {
        personConfig.For(s => s.Name).SetLength(person1.Name.Length + 1);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain($"Name = {person1.Name + Environment.NewLine}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void PrintToString_ShouldWork_WithArrays()
    {
        var collection = new[] { person1, person2, person3 };
        var configuredPrintToString = personConfig.PrintEnumerable(collection);
        configuredPrintToString.Should().Contain($"Name = {person1.Name}");
        configuredPrintToString.Should().Contain($"Name = {person2.Name}");
        configuredPrintToString.Should().Contain($"Name = {person3.Name}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void PrintToString_ShouldWork_WithLists()
    {
        var collection = new List<Person> { person1, person2, person3 };
        personConfig.Exclude(p => p.Id);
        var configuredPrintToString = personConfig.PrintEnumerable(collection);
        configuredPrintToString.Should().Contain($"Name = {person1.Name}");
        configuredPrintToString.Should().Contain($"Name = {person2.Name}");
        configuredPrintToString.Should().Contain($"Name = {person3.Name}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void PrintToString_ShouldWork_WithDictionaries()
    {
        var collection = new Dictionary<Person, int> { { person1, 1 }, { person2, 2 }, { person3, 3 } };
        var configuredPrintToString = personConfig.PrintDictionary(collection);
        configuredPrintToString.Should().Contain($"Name = {person1.Name}");
        configuredPrintToString.Should().Contain($"Name = {person2.Name}");
        configuredPrintToString.Should().Contain($"Name = {person3.Name}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void IgnoreCyclicReference_ShouldIgnore_WithCyclicReference()
    {
        var person = person1;
        person.Parent = person;
        var configuredPrintToString = personConfig.PrintToString(person);
        configuredPrintToString.Should().Contain("New cyclic reference detected");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void PrintToString_ShouldPrintParent_WithConfiguration()
    {
        var person = person1;
        person.Parent = person2;
        personConfig.For<int>().Exclude();
        personConfig.Exclude(p => p.Id);
        var configuredPrintToString = personConfig.PrintToString(person);
        configuredPrintToString.Should().NotContain("Age");
        configuredPrintToString.Should().NotContain("Id");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void PrintToString_ShouldPrintArrays_InsideClass()
    {
        var person = person1;
        person.Aliases = new[] { "Alex", "Memes", "Tupac" };
        var configuredPrintToString = personConfig.PrintToString(person);
        configuredPrintToString.Should().Contain("Age");
        configuredPrintToString.Should().Contain("Memes");
        configuredPrintToString.Should().Contain("Tupac");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void PrintToString_ShouldPrintLists_InsideClass()
    {
        var person = person1;
        person.FavoriteNumbers = new List<int> { 420, 1337, 69 };
        var configuredPrintToString = personConfig.PrintToString(person);
        configuredPrintToString.Should().Contain("420");
        configuredPrintToString.Should().Contain("1337");
        configuredPrintToString.Should().Contain("69");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void PrintToString_ShouldExclude_ExcludedInsideCollections()
    {
        var person = person1;
        person.FavoriteNumbers = new List<int> { 420, 1337, 69 };
        var configuredPrintToString = personConfig.For<int>().Exclude().PrintToString(person);
        configuredPrintToString.Should().NotContain("420");
        configuredPrintToString.Should().NotContain("1337");
        configuredPrintToString.Should().NotContain("69");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void PrintToString_ShouldPrintDictionaries_InsideClass()
    {
        var person = person1;
        person.Tasks = new Dictionary<string, DateTime>
            { { "do homework", DateTime.Today }, { "bake a cake", DateTime.Now } };
        var configuredPrintToString = personConfig.For<int>().Exclude().PrintToString(person);
        configuredPrintToString.Should().Contain($"do homework : {DateTime.Today}");
        configuredPrintToString.Should().Contain($"bake a cake : {DateTime.Now}");
        Console.WriteLine(configuredPrintToString);
    }

    [Test]
    public void PrintToString_ShouldPrint_HardObjects()
    {
        var person = person1;
        person2.Tasks = new Dictionary<string, DateTime>
            { { "make memes", DateTime.Today }, { "create serializer", DateTime.Now } };
        person.Childrens = new[] { person2, person3 };
        personConfig.Exclude(p => p.Id);
        var configuredPrintToString = personConfig.PrintToString(person);
        configuredPrintToString.Should().Contain($"Name = {person1.Name}");
        configuredPrintToString.Should().Contain($"Name = {person2.Name}");
        configuredPrintToString.Should().Contain($"Name = {person3.Name}");
        Console.WriteLine(configuredPrintToString);
    }
    
    [Test]
    public void PrintToString_ShouldNotExclude_PropertiesInNestedObjects()
    {
        person1.Parent = new Person() { Name = "Vasiliy", Age = 20};
        personConfig.Exclude(p => p.Age);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain($"Age = {person1.Parent.Age}");
        Console.WriteLine(configuredPrintToString);
    }
    
    [Test]
    public void PrintToString_ShouldExclude_ConfiguredPropertiesInNestedObjects()
    {
        person1.Parent = new Person() { Name = "Vasiliy", Age = 20};
        personConfig.Exclude(p => p.Parent.Age);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().NotContain($"Age = {person1.Parent.Age}");
        configuredPrintToString.Should().Contain($"Age = {person1.Age}");
        Console.WriteLine(configuredPrintToString);
    }
    
    [Test]
    public void PrintToString_ShouldWork_WithFields()
    {
        personConfig.Exclude(p => p.Weight);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().NotContain("Width");
        Console.WriteLine(configuredPrintToString);
    }
    
    [Test]
    public void Exclude_ShouldExclude_FieldsWithExcludedType()
    {
        personConfig.For<int>().Exclude();
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().NotContain("Width");
        Console.WriteLine(configuredPrintToString);
    }
    
    [Test]
    public void Exclude_ShouldNotExclude_PropertiesWithSameName()
    {
        person1.Location = new Location { Name = "Yekat" };
        personConfig.Exclude(p => p.Name);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain("Yekat");
        Console.WriteLine(configuredPrintToString);
    }
    
    [Test]
    public void SetLength_ShouldNot_CutLineWithSameName()
    {
        person1.Location = new Location { Name = "Yekat" };
        personConfig.For(p => p.Name).SetLength(3);
        var configuredPrintToString = personConfig.PrintToString(person1);
        configuredPrintToString.Should().Contain(person1.Location.Name);
        configuredPrintToString.Should().Contain(person1.Name[..3]);
        configuredPrintToString.Should().NotContain(person1.Name);
        Console.WriteLine(configuredPrintToString);
    }
}